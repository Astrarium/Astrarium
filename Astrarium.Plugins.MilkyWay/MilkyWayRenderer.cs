﻿using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace Astrarium.Plugins.MilkyWay
{
    /// <summary>
    /// Renders Milky Way filled outline on the map
    /// </summary>
    public class MilkyWayRenderer : BaseRenderer
    {
        private readonly MilkyWayCalc milkyWayCalc;
        private readonly ISettings settings;
        private readonly string texturePath;

        public override RendererOrder Order => RendererOrder.Background;

        public MilkyWayRenderer(MilkyWayCalc milkyWayCalc, ISettings settings)
        {
            this.milkyWayCalc = milkyWayCalc;
            this.settings = settings;

            texturePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "MilkyWay.jpg");
        }

        public override void Render(ISkyMap map)
        {
            if (!settings.Get("MilkyWay")) return;
            var prj = map.Projection;

            // nautical twilight: suppose Milky Way is not visible
            if (settings.Get("Atmosphere") && milkyWayCalc.SunAltitude > -12) return;

            const double maxAlpha = 80;
            const double minAlpha = 1;
            const double minFov = 1;

            double maxFov = prj.MaxFov;

            double a = 0;
            double b = maxAlpha;

            if (settings.Get("MilkyWayDimOnZoom"))
            {
                a = -(maxAlpha - minAlpha) / (minFov - maxFov);
                b = -(maxFov * minAlpha - minFov * maxAlpha) / (minFov - maxFov);
            }

            // milky way dimming
            int alpha = Math.Min((int)(a * prj.Fov + b), 255);

            // astronomical twilight: Milky Way is appearing with linear transparency coeff.: 0...1
            if (milkyWayCalc.SunAltitude >= -18 && milkyWayCalc.SunAltitude <= -12)
            {
                alpha = (int)(alpha * (-milkyWayCalc.SunAltitude / 6 - 2));
            }

            if (alpha < minAlpha) return;

            int texture = GL.GetTexture(texturePath, readyCallback: map.Invalidate);
            if (texture <= 0) return;

            var nightMode = settings.Get("NightMode");

            GL.Enable(GL.TEXTURE_2D);
            GL.Enable(GL.CULL_FACE);
            GL.Enable(GL.BLEND);
            GL.BlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);

            if (!prj.FlipVertical ^ prj.FlipHorizontal)
            {
                GL.CullFace(GL.BACK);
            }
            else
            {
                GL.CullFace(GL.FRONT);
            }

            GL.BindTexture(GL.TEXTURE_2D, texture);

            const int steps = 32;

            Color milkyWayColor = Color.FromArgb(205, 225, 255);

            GL.Color4(Color.FromArgb(alpha, milkyWayColor).Tint(nightMode));

            for (double lat = -80; lat <= 90; lat += 10)
            {
                GL.Begin(GL.TRIANGLE_STRIP);

                for (int i = 0; i <= steps; i++)
                {
                    double lon = 360 - i / (double)steps * 360;

                    for (int k = 0; k < 2; k++)
                    {
                        // galactical coordinates of a point, for B1950.0 epoch
                        var gal = new CrdsGalactical(lon, lat - k * 10);

                        // convert to equatorial coordinates for current epoch
                        var eq = Precession.GetEquatorialCoordinates(gal.ToEquatorial(), milkyWayCalc.PrecessionElementsB1950ToCurrent);

                        var p = prj.Project(eq);

                        if (p != null)
                        {
                            double s = (double)i / steps;
                            double t = (90 - (lat - k * 10)) / 180.0;

                            double coef = 1;
                            double ext = 1;
                            if (prj.UseExtinction)
                            {
                                double alt = prj.ToHorizontal(eq).Altitude;
                                ext = alt > 0 ? (1 - (prj.ExtinctionCoefficient - 0.1)) : 0;
                                coef = alt > 0 ? Math.Cos(Angle.ToRadians(90 - alt)) : 1;
                            }
                            GL.Color4(Color.FromArgb((int)(alpha * ext * coef), milkyWayColor).Tint(nightMode));

                            GL.TexCoord2(s, t);
                            GL.Vertex2(p.X, p.Y);
                        }
                        else
                        {
                            GL.End();
                            GL.Begin(GL.TRIANGLE_STRIP);
                            break;
                        }
                    }
                }
                GL.End();
            }

            GL.Disable(GL.TEXTURE_2D);
            GL.Disable(GL.CULL_FACE);
            GL.Disable(GL.BLEND);
        }
    }
}
