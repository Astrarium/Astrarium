using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Astrarium.Plugins.Hipparcos
{
    public class StarsRenderer : CelestialObjectRenderer<Star>
    {
        private readonly ISky sky;
        private readonly ISkyMap map;
        private readonly StarsCalc starsCalc;
        private readonly ISettings settings;

        private const int limitBayerNames = 20;
        private const int limitProperNames = 20;
        private const int limitFlamsteedNames = 10;

        private Font defaultFont;

        public StarsRenderer(ISky sky, ISkyMap map, StarsCalc starsCalc, ISettings settings) : base(map)
        {
            this.sky = sky;
            this.map = map;
            this.starsCalc = starsCalc;
            this.settings = settings;

            // TODO: move to settings
            defaultFont = new Font("Arial", 7);
        }

        public override void Render(ISkyMap map)
        {
            var prj = map.Projection;
            bool nightMode = settings.Get("NightMode");

            GL.Enable(GL.BLEND);
            GL.BlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);
            GL.Enable(GL.POINT_SMOOTH);
            GL.Enable(GL.LINE_SMOOTH);
            GL.Hint(GL.POINT_SMOOTH_HINT, GL.NICEST);
            GL.Hint(GL.LINE_SMOOTH_HINT, GL.NICEST);
            GL.Enable(GL.CULL_FACE);

            if (!prj.FlipVertical ^ prj.FlipHorizontal)
            {
                GL.CullFace(GL.BACK);
            }
            else
            {
                GL.CullFace(GL.FRONT);
            }

            double maxFov = prj.MaxFov * 0.7;
            double fov = prj.RealFov;

            // equatorial coordinates of screen center for current epoch
            CrdsEquatorial eqCenter = prj.WithoutRefraction(prj.CenterEquatorial);
            CrdsEquatorial eqCenter0 = Precession.GetEquatorialCoordinates(eqCenter, starsCalc.PrecessionalElements0);

            if (settings.Get("ConstLines"))
            {
                var linePen = new Pen(settings.Get<Color>("ColorConstLines").Tint(nightMode), 0.5f) { DashStyle = DashStyle.Dot };

                foreach (var line in sky.ConstellationLines)
                {
                    var s1 = starsCalc.GetStar(prj.Context, line.Item1);
                    var s2 = starsCalc.GetStar(prj.Context, line.Item2);

                    if (Angle.Separation(eqCenter, s1.Equatorial) < maxFov &&
                        Angle.Separation(eqCenter, s2.Equatorial) < maxFov)
                    {
                        var p1 = prj.Project(s1.Equatorial);
                        var p2 = prj.Project(s2.Equatorial);
                        if (p1 != null && p2 != null)
                        {
                            GL.DrawLine(p1, p2, linePen);
                        }
                    }
                }
            }

            if (settings.Get("Stars"))
            {
                float daylightFactor = map.DaylightFactor;

                // no stars if the Sun above horizon
                if (daylightFactor == 1) return;

                float starDimming = 1 - daylightFactor;
                float minStarSize = daylightFactor * 10; // empiric

                var fontStarNames = settings.Get<Font>("StarsLabelsFont");
                var color = settings.Get<Color>("ColorStarsLabels").Tint(nightMode);
                var brushStarNames = new SolidBrush(color);
                bool starsLabels = settings.Get("StarsLabels");
                bool properNames = settings.Get("StarsProperNames");
                float starsScalingFactor = (float)settings.Get("StarsScalingFactor", 1m);
                bool markVariables = settings.Get("StarsMarkVariables");
                bool markMultiples = settings.Get("StarsMarkMultiples");

                Func<float, bool> filterByStarSize = (mag) =>
                {
                    if (mag > prj.MagLimit)
                        return false;

                    float size = prj.GetPointSize(mag) * starDimming;
                    return size >= minStarSize;
                };

                var stars = GetCelestialObjectsWithCache(prj.Context, eqCenter0, fov, filterByStarSize);

                foreach (var star in stars)
                {
                    double alt = prj.ToHorizontal(star.Equatorial).Altitude;
                    float size = prj.GetPointSize(star.Magnitude, altitude: alt) * starDimming;
                    if (size > minStarSize)
                    {
                        var p = prj.Project(star.Equatorial);

                        if (prj.IsInsideScreen(p))
                        {
                            float sz = size * starsScalingFactor;
                            var clr = GetStarColor(star.Color).Tint(nightMode);

                            GL.PointSize(sz);
                            GL.Color3(clr);

                            GL.Begin(GL.POINTS);
                            GL.Vertex2(p.X, p.Y);
                            GL.End();

                            if (markMultiples && sz > 2 && star.IsMultiple)
                            {
                                GL.DrawLine(new PointF((float)p.X - sz / 2 - 3, (float)p.Y), new PointF((float)p.X + sz / 2 + 3, (float)p.Y), new Pen(clr, 0.2f));
                            }

                            if (markVariables && star.IsVariable)
                            {
                                GL.DrawEllipse(p, new Pen(Color.FromArgb(50, clr)), sz / 2 + 3);
                            }

                            if (starsLabels)
                            {
                                DrawStarName(prj, fontStarNames, brushStarNames, properNames, p, star, sz);
                            }

                            map.AddDrawnObject(p, star);
                        }
                    }
                }
            }

            GL.Disable(GL.POINT_SMOOTH);
            GL.Disable(GL.BLEND);
            GL.Disable(GL.CULL_FACE);
        }

        protected override Color GetStarColor(char spClass)
        {
            if (settings.Get("StarsColors"))
            {
                return base.GetStarColor(spClass);
            }
            else
            {
                return Color.White;
            }
        }

        protected override IEnumerable<Star> GetCelestialObjectsWithoutCache(SkyContext context, CrdsEquatorial eq0, double angle, Func<float, bool> magFilter)
        {
            return starsCalc.GetStars(context, eq0, angle, magFilter);
        }

        /// <summary>
        /// Draws star name
        /// </summary>
        private void DrawStarName(Projection prj, Font font, Brush brush, bool properNames, PointF point, Star s, float diam)
        {
            // Star has proper name
            if (properNames && s.ProperName != null && prj.Fov < limitProperNames)
            {
                map.DrawObjectLabel(s.ProperName, font, brush, point, diam);
                return;
            }

            // Star has Bayer name (greek letter)
            if (prj.Fov < limitBayerNames)
            {
                string bayerName = starsCalc.GetBayerNameForPrint(s);
                if (!string.IsNullOrEmpty(bayerName))
                {
                    map.DrawObjectLabel(bayerName, font, brush, point, diam);
                    return;
                }
            }

            // Star has Flamsteed number
            if (prj.Fov < limitFlamsteedNames)
            {
                if (s.Flamsteed > 0)
                {
                    map.DrawObjectLabel(((int)s.Flamsteed).ToString(), font, brush, point, diam);
                    return;
                }
            }

            // Star doesn't have any names
            if (prj.Fov < 2)
            {
                map.DrawObjectLabel($"HIP {s.HIP}", defaultFont, brush, point, diam);
            }
        }

        public override RendererOrder Order => RendererOrder.Stars;
    }
}
