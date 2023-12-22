using Astrarium.Algorithms;
using Astrarium.Types;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Astrarium.Plugins.BrightStars
{
    public class StarsRenderer : BaseRenderer
    {
        private readonly ISky sky;
        private readonly ISkyMap map;
        private readonly StarsCalc starsCalc;
        private readonly ISettings settings;

        private Lazy<TextRenderer> textRenderer = new Lazy<TextRenderer>(() => new TextRenderer(128, 32));

        private const int limitBayerNames = 20;
        private const int limitProperNames = 20;
        private const int limitFlamsteedNames = 10;
        private const int limitVarNames = 5;

        public StarsRenderer(ISky sky, ISkyMap map, StarsCalc starsCalc, ISettings settings)
        {
            this.sky = sky;
            this.map = map;
            this.starsCalc = starsCalc;
            this.settings = settings;
        }

        public override void Render(ISkyMap map)
        {
            var prj = map.Projection;
            bool nightMode = settings.Get("NightMode");

            GL.Enable(EnableCap.PointSmooth);
            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Enable(EnableCap.CullFace);

            if (!prj.FlipVertical ^ prj.FlipHorizontal)
            {
                GL.CullFace(CullFaceMode.Back);
            }
            else
            {
                GL.CullFace(CullFaceMode.Front);
            }

            var allStars = starsCalc.Stars;

            double maxFov = prj.MaxFov * 0.7;

            // fov
            double fov = prj.Fov * Math.Max(prj.ScreenWidth, prj.ScreenHeight) / Math.Min(prj.ScreenWidth, prj.ScreenHeight) + 1e-2;

            // equatorial coordinates of screen center for current epoch
            CrdsEquatorial eqCenter = prj.WithoutRefraction(prj.CenterEquatorial);

            if (settings.Get("ConstLines"))
            {
                var linePen = new Pen(settings.Get<Color>("ColorConstLines").Tint(nightMode), 1) { DashStyle = DashStyle.Dot };

                foreach (var line in sky.ConstellationLines)
                {
                    var s1 = allStars.ElementAt(line.Item1);
                    var s2 = allStars.ElementAt(line.Item2);

                    if (Angle.Separation(eqCenter, s1.Equatorial) < maxFov &&
                        Angle.Separation(eqCenter, s2.Equatorial) < maxFov)
                    {
                        var p1 = prj.Project(s1.Equatorial);
                        var p2 = prj.Project(s2.Equatorial);
                        if (p1 != null && p2 != null)
                        {
                            Primitives.DrawLine(p1, p2, linePen);
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
                float minStarSize = daylightFactor * 3; // empiric

                var fontStarNames = settings.Get<Font>("StarsLabelsFont");
                var color = settings.Get<Color>("ColorStarsLabels").Tint(nightMode);
                var brushStarNames = new SolidBrush(color);
                bool properNames = settings.Get("StarsProperNames");
                float starsScalingFactor = (float)settings.Get<decimal>("StarsScalingFactor", 1);

                float magLimit = prj.MagLimit;
                var stars = starsCalc.GetStars(eqCenter, fov, m => m <= magLimit);

                foreach (var star in stars)
                {
                    float size = prj.GetPointSize(star.Magnitude) * starDimming;
                    if (size > minStarSize)
                    {
                        var p = prj.Project(star.Equatorial);

                        if (prj.IsInsideScreen(p))
                        {
                            GL.PointSize(size * starsScalingFactor);
                            GL.Color3(GetColor(star.Color).Tint(nightMode));

                            GL.Begin(PrimitiveType.Points);
                            GL.Vertex2(p.X, p.Y);
                            GL.End();

                            map.AddDrawnObject(p, star, size);

                            DrawStarName(prj, fontStarNames, brushStarNames, properNames, p, star, size);
                        }
                    }
                }
            }

            GL.Disable(EnableCap.PointSmooth);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.CullFace);
        }        

        private Color GetColor(char spClass)
        {
            if (settings.Get("StarsColors"))
            {
                switch (spClass)
                {
                    case 'O':
                    case 'W':
                        return Color.LightBlue;
                    case 'B':
                        return Color.LightCyan;
                    case 'A':
                        return Color.White;
                    case 'F':
                        return Color.LightYellow;
                    case 'G':
                        return Color.Yellow;
                    case 'K':
                        return Color.Orange;
                    case 'M':
                        return Color.OrangeRed;
                    default:
                        return Color.White;
                }
            }
            else
            {
                return Color.White;
            }
        }

        /// <summary>
        /// Draws star name
        /// </summary>
        private void DrawStarName(Projection prj, Font font, Brush brush, bool properNames, PointF point, Star s, float diam)
        {
            // Star has proper name
            if (properNames && s.ProperName != null && prj.Fov < limitProperNames)
            {
                map.DrawObjectLabel(textRenderer.Value, s.ProperName, font, brush, point, diam);
                return;
            }

            // Star has Bayer name (greek letter)
            if (prj.Fov < limitBayerNames)
            {
                string bayerName = s.BayerName;
                if (bayerName != null)
                {
                    map.DrawObjectLabel(textRenderer.Value, bayerName, font, brush, point, diam);
                    return;
                }
            }

            // Star has Flamsteed number
            if (prj.Fov < limitFlamsteedNames)
            {
                string flamsteedNumber = s.FlamsteedNumber;
                if (flamsteedNumber != null)
                {
                    map.DrawObjectLabel(textRenderer.Value, flamsteedNumber, font, brush, point, diam);
                    return;
                }
            }

            // Star has variable id
            if (prj.Fov < limitVarNames && s.VariableName != null)
            {
                string varName = s.VariableName.Split(' ')[0];
                if (!varName.All(char.IsDigit))
                {
                    map.DrawObjectLabel(textRenderer.Value, varName, font, brush, point, diam);
                    return;
                }
            }

            // Star doesn't have any names
            if (prj.Fov < 2)
            {
                map.DrawObjectLabel(textRenderer.Value, $"HR {s.Number}", font, brush, point, diam);
            }
        }

        public override RendererOrder Order => RendererOrder.Stars;
    }
}
