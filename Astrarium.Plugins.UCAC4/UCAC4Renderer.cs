﻿using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Astrarium.Plugins.UCAC4
{
    public class UCAC4Renderer : BaseRenderer
    {
        private Font fontNames;
        private readonly UCAC4Catalog catalog;
        private readonly ISettings settings;
        private readonly ISkyMap map;
        private readonly List<UCAC4Star> cache = new List<UCAC4Star>();
        private readonly object locker = new object();
        private bool isRequested = false;
        private int requestHash;

        public override RendererOrder Order => RendererOrder.Stars;

        public UCAC4Renderer(ISkyMap map, UCAC4Catalog catalog, ISettings settings)
        {
            this.map = map;
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

            float magLimit = Math.Min(float.MaxValue, (float)(-1.73494 * Math.Log(0.000462398 * fov)));

            if (magLimit > 10 && settings.Get("Stars") && settings.Get("UCAC4"))
            {
                float starDimming = 1 - daylightFactor;
                float minStarSize = Math.Max(0.5f, daylightFactor * 10);

                bool nightMode = settings.Get("NightMode");
                bool isLabels = settings.Get("StarsLabels") && fov < 1 / 60d;
                Brush brushNames = new SolidBrush(settings.Get<Color>("ColorStarsLabels").Tint(nightMode));
                float starsScalingFactor = (float)settings.Get<decimal>("StarsScalingFactor", 1);

                GL.Enable(GL.POINT_SMOOTH);
                GL.Enable(GL.BLEND);
                GL.BlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);
                GL.Hint(GL.POINT_SMOOTH_HINT, GL.NICEST);

                CrdsEquatorial eqCenter = prj.WithoutRefraction(prj.CenterEquatorial);
                CrdsEquatorial eqCenter0 = Precession.GetEquatorialCoordinates(eqCenter, catalog.PrecessionElements0);

                Func<float, bool> FilterByStarSize = (mag) =>
                {
                    if (mag > magLimit)
                        return false;

                    float size = prj.GetPointSize(mag) * starDimming;
                    return size >= minStarSize;
                };

                var stars = GetStars(prj.Context, eqCenter0, fov, FilterByStarSize);

                foreach (var star in stars)
                {
                    double alt = prj.ToHorizontal(star.Equatorial).Altitude;
                    float size = prj.GetPointSize(star.Magnitude, altitude: alt) * starDimming;
                    if (size < minStarSize) continue;

                    var p = prj.Project(star.Equatorial);

                    if (prj.IsInsideScreen(p))
                    {
                        GL.PointSize(size * starsScalingFactor);
                        GL.Color3(GetColor(star.SpectralClass).Tint(nightMode));

                        GL.Begin(GL.POINTS);
                        GL.Vertex2(p.X, p.Y);
                        GL.End();

                        map.AddDrawnObject(p, star);

                        if (isLabels && size > 2)
                        {
                            map.DrawObjectLabel(star.Names.First(), fontNames, brushNames, p, size);
                        }
                    }
                }
            }
        }

        private List<UCAC4Star> GetStars(SkyContext context, CrdsEquatorial eq0, double angle, Func<float, bool> magFilter)
        {
            _ = Task.Run(() =>
            {
                int hash = $"{eq0.Alpha}{eq0.Delta}{angle}".GetHashCode();
                if (!isRequested && hash != requestHash)
                {
                    requestHash = hash;
                    isRequested = true;
                    var stars = catalog.GetStars(context, eq0, angle, magFilter).ToList();

                    lock (locker)
                    {
                        cache.Clear();
                        cache.AddRange(stars);
                        map.Invalidate();
                    }

                    isRequested = false;
                }
            });

            lock (locker)
            {
                return new List<UCAC4Star>(cache);
            }
        }

        private Color GetColor(char spClass)
        {
            Color starColor;
            if (settings.Get("StarsColors"))
            {
                switch (spClass)
                {
                    case 'O':
                    case 'W':
                        starColor = Color.LightBlue;
                        break;
                    case 'B':
                        starColor = Color.LightCyan;
                        break;
                    case 'A':
                        starColor = Color.White;
                        break;
                    case 'F':
                        starColor = Color.LightYellow;
                        break;
                    case 'G':
                        starColor = Color.Yellow;
                        break;
                    case 'K':
                        starColor = Color.Orange;
                        break;
                    case 'M':
                        starColor = Color.OrangeRed;
                        break;
                    default:
                        starColor = Color.White;
                        break;
                }
            }
            else
            {
                starColor = Color.White;
            }

            return starColor;
        }
    }
}
