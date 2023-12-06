using Astrarium.Algorithms;
using Astrarium.Types;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcColor);

            // if only one of the flipping enabled
            if (!prj.FlipVertical ^ prj.FlipHorizontal)
            {
                GL.CullFace(CullFaceMode.Back);
            }
            else
            {
                GL.CullFace(CullFaceMode.Front);
            }

            double stepAlt = 10;
            double stepAzi = 10;

            for (double alt = -80; alt <= 90; alt += stepAlt)
            {
                GL.Begin(PrimitiveType.QuadStrip);

                for (double azi = 0; azi <= 360; azi += stepAzi)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        var hor = new CrdsHorizontal(azi, alt - (k * stepAlt));

                        var p = prj.Project(hor);

                        if (p != null)
                        {
                            if (hor.Altitude < 0)
                            {
                                GL.Color3(calc.GetColor(new CrdsHorizontal(hor.Azimuth, 90)));
                            }
                            else
                            {
                                GL.Color3(calc.GetColor(hor));
                            }

                            GL.Vertex2(p.X, p.Y);
                        }
                        else
                        {
                            GL.End();
                            GL.Begin(PrimitiveType.QuadStrip);
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

                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                stepAlt = lightPollutionAltitude / 9;
                for (double alt = stepAlt; alt <= lightPollutionAltitude + 1e-6; alt += stepAlt)
                {
                    GL.Begin(PrimitiveType.QuadStrip);

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
                                GL.Begin(PrimitiveType.QuadStrip);
                                break;
                            }
                        }
                    }

                    GL.End();
                }
            }
        }
    }
}
