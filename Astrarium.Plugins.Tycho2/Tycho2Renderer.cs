using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Tycho2
{
    public class Tycho2Renderer : CelestialObjectRenderer<Tycho2Star>
    {
        private readonly ITycho2Catalog tycho2;
        private readonly ISettings settings;

        private Font fontNames;

        public override RendererOrder Order => RendererOrder.Stars;

        private readonly ISkyMap map;

        public Tycho2Renderer(ISkyMap map, ITycho2Catalog tycho2, ISettings settings) : base(map)
        {
            this.map = map;
            this.tycho2 = tycho2;
            this.settings = settings;
            // TODO: move to settings
            fontNames = new Font("Arial", 7);
        }

        public override void Render(ISkyMap map)
        {
            Projection prj = map.Projection;
            double fov = prj.RealFov;
            float magLimit = prj.MagLimit;

            if (magLimit > 6 && settings.Get("Stars") && settings.Get("Tycho2"))
            {
                float daylightFactor = map.DaylightFactor;

                // no stars if the Sun above horizon
                if (daylightFactor == 1) return;

                float starDimming = 1 - daylightFactor;
                float minStarSize = Math.Max(0.5f, daylightFactor * 10); // empiric

                var nightMode = settings.Get("NightMode");
                bool isLabels = settings.Get<bool>("StarsLabels");
                float starsScalingFactor = (float)settings.Get<decimal>("StarsScalingFactor", 1);
                Brush brushNames = new SolidBrush(settings.Get<Color>("ColorStarsLabels").Tint(nightMode));

                GL.Enable(GL.POINT_SMOOTH);
                GL.Enable(GL.BLEND);
                GL.BlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);
                GL.Hint(GL.POINT_SMOOTH_HINT, GL.NICEST);

                CrdsEquatorial eqCenter = prj.WithoutRefraction(prj.CenterEquatorial);
                CrdsEquatorial eqCenter0 = Precession.GetEquatorialCoordinates(eqCenter, tycho2.PrecessionalElements0);

                Func<float, bool> filterByStarSize = (mag) =>
                {
                    if (mag > magLimit)
                        return false;

                    float size = prj.GetPointSize(mag) * starDimming;
                    return size >= minStarSize;
                };

                var stars = GetCelestialObjectsWithCache(prj.Context, eqCenter0, fov, filterByStarSize);

                foreach (var star in stars)
                {
                    double alt = prj.ToHorizontal(star.Equatorial).Altitude;
                    float size = prj.GetPointSize(star.Magnitude, altitude: alt) * starDimming;
                    if (size < minStarSize) continue;

                    var p = prj.Project(star.Equatorial);

                    if (prj.IsInsideScreen(p))
                    {
                        float sz = size * starsScalingFactor;

                        GL.PointSize(sz);
                        GL.Color3(GetStarColor(star.SpectralClass).Tint(nightMode));

                        GL.Begin(GL.POINTS);
                        GL.Vertex2(p.X, p.Y);
                        GL.End();

                        map.AddDrawnObject(p, star);

                        if (isLabels && prj.Fov < 1 && size > 3)
                        {
                            map.DrawObjectLabel(star.Names.First(), fontNames, brushNames, p, sz);
                        }
                    }
                }
            }
        }

        protected override IEnumerable<Tycho2Star> GetCelestialObjectsWithoutCache(SkyContext context, CrdsEquatorial eq0, double angle, Func<float, bool> magFilter)
        {
            return tycho2.GetStars(context, eq0, angle, magFilter);
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
    }
}
