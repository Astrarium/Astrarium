using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Drawing;

namespace Astrarium.Plugins.Atmosphere
{
    public class FogRenderer : BaseRenderer
    {
        public override RendererOrder Order => RendererOrder.Surround;

        private readonly ISettings settings;

        public FogRenderer(ISettings settings)
        {
            this.settings = settings;
        }

        public override void Render(ISkyMap map)
        {
            if (!settings.Get("Ground") ||
                !settings.Get("Atmosphere") ||
                !settings.Get("Fog") ||
                map.DaylightFactor == 1) return;

            // fog altitude expressed in degrees
            double fogAltitude = (double)settings.Get<decimal>("FogAltitude");

            // fog spreading expressed in percents -> transform to degrees
            double fogSpreading = (double)(settings.Get<decimal>("FogSpreading") / 100 * 90);

            // fog intensity expressed in percents -> transform to normalized value [0...1]
            double fogIntensity = (double)(settings.Get<decimal>("FogIntensity") / 100);

            if (fogSpreading == 0 && fogAltitude == 0) return;
            if (fogSpreading == 0) fogSpreading = 1;
            if (fogAltitude == 0) fogAltitude = 1;

            // reference fog color, tinted to night mode if required
            Color referenceColor = Color.FromArgb(64, 128, 255).Tint(settings.Get("NightMode"));

            // fog color depending on intensity
            byte r = (byte)(fogIntensity * referenceColor.R);
            byte g = (byte)(fogIntensity * referenceColor.G);
            byte b = (byte)(fogIntensity * referenceColor.B);

            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            var prj = map.Projection;

            if (!prj.FlipVertical ^ prj.FlipHorizontal)
            {
                GL.CullFace(CullFaceMode.Back);
            }
            else
            {
                GL.CullFace(CullFaceMode.Front);
            }

            const int steps = 5;

            Func<int, double> GetAltitude = (int i) => Math.Abs(i) / (double)steps * (i < 0 ? -fogSpreading : fogAltitude);

            for (int i = -steps; i <= steps; i++)
            {
                double alt = GetAltitude(i);
                double alt0 = GetAltitude(i - 1);

                GL.Begin(PrimitiveType.QuadStrip);

                for (double azi = 0; azi <= 360; azi += 10)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        CrdsHorizontal hor = new CrdsHorizontal(azi, k == 0 ? alt : alt0);
                        Vec2 p = prj.Project(hor);
                        if (p != null && hor.Altitude >= -fogSpreading && hor.Altitude <= fogAltitude)
                        {
                            double amplitude = hor.Altitude < 0 ? fogSpreading : fogAltitude;
                            byte alpha = (byte)((1 - map.DaylightFactor) * fogIntensity * 255 * ((amplitude - Math.Abs(hor.Altitude)) / amplitude));
                            Color color = Color.FromArgb(alpha, r, g, b);
                            GL.Color4(color);
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

            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);
        }
    }
}
