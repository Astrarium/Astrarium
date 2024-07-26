using Astrarium.Algorithms;
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

            // maximal displayed brightness of Milky Way
            const double maxAlpha = 80;

            // minimal displayed brightness of Milky Way
            const double minAlpha = 1;

            // minimal FOV to display Milky Way if "MilkyWayDimOnZoom" setting is enabled
            const double minFov = 1;

            // Milky Way brightness
            int alpha = (int)maxAlpha;

            // max FOV
            double maxFov = prj.MaxFov;

            // dimming on zoom
            if (settings.Get("MilkyWayDimOnZoom"))
            {
                double a = 0;
                double b = maxAlpha;

                a = -(maxAlpha - minAlpha) / (minFov - maxFov);
                b = -(maxFov * minAlpha - minFov * maxAlpha) / (minFov - maxFov);

                alpha = Math.Min((int)(a * prj.Fov + b), 255);
            }

            // dimming in atmosphere depending on Sun altitude
            // astronomical twilight: Milky Way is appearing with linear transparency coeff.: 0...1
            if (settings.Get("Atmosphere") && milkyWayCalc.SunAltitude >= -18 && milkyWayCalc.SunAltitude <= -12)
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

                            double altDimming = 1;
                            double extinctionDimming = 1;

                            double alt = prj.ToHorizontal(eq).Altitude;

                            // if extinction is used, dim areas around horizon
                            if (prj.UseExtinction)
                            {
                                if (alt >= 0)
                                {
                                    const double minExt = 0.1;
                                    const double maxExt = 0.6;
                                    extinctionDimming = 1 - (prj.ExtinctionCoefficient - minExt) / (maxExt - minExt);
                                    altDimming = Math.Cos(Angle.ToRadians(90 - alt));
                                }
                                else
                                {
                                    altDimming = 0;
                                    extinctionDimming = 0;
                                }
                            }

                            GL.Color4(Color.FromArgb((int)(alpha * extinctionDimming * altDimming), milkyWayColor).Tint(nightMode));

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
