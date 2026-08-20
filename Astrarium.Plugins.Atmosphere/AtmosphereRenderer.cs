using Astrarium.Algorithms;
using Astrarium.Types;
using System.Drawing;
using static Astrarium.Algorithms.Angle;
using static System.Math;

namespace Astrarium.Plugins.Atmosphere
{
    public class AtmosphereRenderer : BaseRenderer
    {
        public override RendererOrder Order => RendererOrder.Atmosphere;

        private readonly AtmosphereCalculator calc;
        private readonly ISettings settings;

        public AtmosphereRenderer(AtmosphereCalculator calc, ISettings settings)
        {
            this.calc = calc;
            this.settings = settings;
        }

        public override void Render(ISkyMap map)
        {
            if (!settings.Get("Atmosphere")) return;

            var prj = map.Projection;

            GL.Enable(GL.CULL_FACE);
            GL.Enable(GL.BLEND);
            GL.Disable(GL.TEXTURE_2D);
            GL.BlendFunc(GL.ONE, GL.ONE_MINUS_SRC_COLOR);

            // if only one of the flipping enabled
            if (!prj.FlipVertical ^ prj.FlipHorizontal)
            {
                GL.CullFace(GL.BACK);
            }
            else
            {
                GL.CullFace(GL.FRONT);
            }

            double stepAlt = 5;
            double stepAzi = 10;

            for (double B = stepAlt -90; B <= 90; B += stepAlt)
            {
                GL.Begin(GL.QUAD_STRIP);
                for (double L = 0; L <= 360; L += stepAzi)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        var hor = SolicentricToHorizontal(new CrdsHorizontal(L, B - k * stepAlt));
                        var p = prj.Project(hor);
                        if (p != null)
                        {
                            GL.Color3(calc.GetColor(hor));
                            GL.Vertex2(p.X, p.Y);
                        }
                        else
                        {
                            GL.End();
                            GL.Begin(GL.QUAD_STRIP);
                            break;
                        }
                    }
                }

                GL.End();
            }

            // Light pollution overlay
            if (map.DaylightFactor < 1 && settings.Get("LightPollution"))
            {
                double lightPollutionAltitude = (double)settings.Get<decimal>("LightPollutionAltitude");
                if (lightPollutionAltitude == 0) return;

                double lightPollutionIntensity = (double)settings.Get<decimal>("LightPollutionIntensity");
                double lightPolutionTone = (double)settings.Get<decimal>("LightPollutionTone");

                byte regGreenComponent = (byte)(lightPollutionIntensity / 100 * 255);
                byte blueComponent = (byte)(regGreenComponent * (1 - lightPolutionTone / 100));
                Color color = Color.FromArgb(regGreenComponent, regGreenComponent, blueComponent).Tint(settings.Get("NightMode"));
                double intensity = (1 - map.DaylightFactor) * (lightPollutionIntensity / 100);

                GL.BlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);

                stepAlt = lightPollutionAltitude / 9;
                for (double alt = stepAlt; alt <= lightPollutionAltitude + 1e-6; alt += stepAlt)
                {
                    GL.Begin(GL.QUAD_STRIP);

                    for (double azi = 0; azi <= 360; azi += stepAzi)
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            var hor = new CrdsHorizontal(azi, alt - (k * stepAlt));
                            var p = prj.Project(hor);
                            if (p != null)
                            {
                                byte bInt = (byte)(255 * intensity * ((lightPollutionAltitude - hor.Altitude) / lightPollutionAltitude));
                                GL.Color4(Color.FromArgb(bInt, color));
                                GL.Vertex2(p.X, p.Y);
                            }
                            else
                            {
                                GL.End();
                                GL.Begin(GL.QUAD_STRIP);
                                break;
                            }
                        }
                    }

                    GL.End();
                }
            }
        }

        /// <summary>
        /// Converts "solicentric" horizontal coordinates to Alt/Az coordinates
        /// </summary>
        /// <param name="p">Solicentric coordinates of a point</param>
        /// <returns>Horizontal coordinates of a point</returns>
        private CrdsHorizontal SolicentricToHorizontal(CrdsHorizontal p)
        {
            double sunAz = ToRadians(calc.SolarCoordinates.Azimuth);
            double sunAlt = ToRadians(calc.SolarCoordinates.Altitude);
            double pointA = ToRadians(p.Azimuth);
            double pointZ = ToRadians(90 + p.Altitude);
            double sinAlt = Sin(sunAlt) * Cos(pointZ) + Cos(sunAlt) * Sin(pointZ) * Cos(pointA);
            double pointAlt = Asin(sinAlt);
            double cosDeltaAz = (Cos(pointZ) - Sin(sunAlt) * Sin(pointAlt)) / (Cos(sunAlt) * Cos(pointAlt));
            double sinDeltaAz = (Sin(pointZ) * Sin(pointA)) / Cos(pointAlt);
            double deltaAz = Atan2(sinDeltaAz, cosDeltaAz);
            double pointAz = sunAz + deltaAz;

            return new CrdsHorizontal(To360(ToDegrees(pointAz)), ToDegrees(pointAlt));
        }
    }
}
