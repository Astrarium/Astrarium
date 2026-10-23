using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Astrarium.Plugins.UCAC4
{
    public class UCAC4Renderer : CelestialObjectRenderer<UCAC4Star>
    {
        private Font fontNames;
        private readonly UCAC4Catalog catalog;
        private readonly ISettings settings;

        public override RendererOrder Order => RendererOrder.Stars;

        public UCAC4Renderer(ISkyMap map, UCAC4Catalog catalog, ISettings settings) : base(map)
        {
            this.catalog = catalog;
            this.settings = settings;
            // TODO: move to settings
            fontNames = new Font("Arial", 7);
        }

        public override void Render(ISkyMap map)
        {
            float daylightFactor = map.DaylightFactor;

            // no stars if the Sun above horizon
            if (daylightFactor == 1) return;

            var prj = map.Projection;
            double fov = prj.RealFov;
            float magLimit = prj.MagLimit;

            if (magLimit > 10 && settings.Get("Stars") && settings.Get("UCAC4"))
            {
                float starDimming = 1 - daylightFactor;
                float minStarSize = Math.Max(0.5f, daylightFactor * 10);

                bool nightMode = settings.Get("NightMode");
                bool isLabels = settings.Get("StarsLabels") && fov < 1 / 60d;
                Brush brushNames = new SolidBrush(settings.Get<Color>("ColorStarsLabels").Tint(nightMode));
                float starsScalingFactor = (float)settings.Get<decimal>("StarsScalingFactor", 1);
                bool useMagFilter = settings.Get("UCAC4UseMagLimit", true);
                float magFilter = (float)settings.Get("UCAC4MagLimit", 6.5m);
                bool hideUnknownMag = settings.Get("UCAC4HideUnknownMag");

                GL.Enable(GL.POINT_SMOOTH);
                GL.Enable(GL.BLEND);
                GL.BlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);
                GL.Hint(GL.POINT_SMOOTH_HINT, GL.NICEST);

                CrdsEquatorial eqCenter = prj.WithoutRefraction(prj.CenterEquatorial);
                CrdsEquatorial eqCenter0 = Precession.GetEquatorialCoordinates(eqCenter, catalog.PrecessionElements0);

                Func<float, bool> filterByStarSize = (mag) =>
                {
                    if (mag == 20.0f && hideUnknownMag)
                        return false;

                    if (mag == 20.0f && !hideUnknownMag)
                        return magLimit >= 15;

                    if (mag > magLimit)
                        return false;

                    if (useMagFilter && mag < magFilter)
                        return false;

                    float size = prj.GetPointSize(mag) * starDimming;
                    return size >= minStarSize;
                };

                var stars = GetCelestialObjectsWithCache(prj.Context, eqCenter0, fov, filterByStarSize);

                foreach (var star in stars)
                {
                    double alt = prj.ToHorizontal(star.Equatorial).Altitude;
                    float size = prj.GetPointSize(star.Magnitude, altitude: alt) * starDimming;

                    if (star.Magnitude == 20.0f && magLimit > 15)
                    {
                        size = 1;
                    }

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

                        if (isLabels && size > 2)
                        {
                            map.DrawObjectLabel(star.Names.First(), fontNames, brushNames, p, sz);
                        }
                    }
                }
            }
        }

        protected override IEnumerable<UCAC4Star> GetCelestialObjectsWithoutCache(SkyContext context, CrdsEquatorial eq0, double angle, Func<float, bool> magFilter)
        {
            return catalog.GetStars(context, eq0, angle, magFilter);
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
