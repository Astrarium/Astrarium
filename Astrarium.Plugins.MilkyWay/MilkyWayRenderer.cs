using Astrarium.Algorithms;
using Astrarium.Types;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Astrarium.Plugins.MilkyWay
{
    /// <summary>
    /// Renders Milky Way filled outline on the map
    /// </summary>
    public class MilkyWayRenderer : BaseRenderer
    {
        private readonly MilkyWayCalc milkyWayCalc;
        private readonly ISky sky;
        private readonly ISettings settings;
        private readonly ITextureManager textureManager;
        private readonly string texturePath;

        public override RendererOrder Order => RendererOrder.Background;

        public MilkyWayRenderer(MilkyWayCalc milkyWayCalc, ITextureManager textureManager, ISky sky, ISettings settings)
        {
            this.milkyWayCalc = milkyWayCalc;
            this.textureManager = textureManager;
            this.sky = sky;
            this.settings = settings;

            texturePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data", "MilkyWay.jpg");
        }

        public override void Render(ISkyMap map)
        {
            if (!settings.Get("MilkyWay")) return;
            var prj = map.Projection;

            // nautical twilight: suppose Milky Way is not visible
            if (settings.Get("Atmosphere") && milkyWayCalc.SunAltitude > -12) return;

            const double maxAlpha = 60;
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

            var nightMode = settings.Get("NightMode");

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            if (!prj.FlipVertical ^ prj.FlipHorizontal)
            {
                GL.CullFace(CullFaceMode.Back);
            }
            else
            {
                GL.CullFace(CullFaceMode.Front);
            }

            GL.BindTexture(TextureTarget.Texture2D, textureManager.GetTexture(texturePath));

            const int steps = 32;

            GL.Color4(Color.FromArgb(alpha, 205, 225, 255).Tint(nightMode));

            for (double lat = -80; lat <= 90; lat += 10)
            {
                GL.Begin(PrimitiveType.TriangleStrip);

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

                            GL.TexCoord2(s, t);
                            GL.Vertex2(p.X, p.Y);
                        }
                        else
                        {
                            GL.End();
                            GL.Begin(PrimitiveType.TriangleStrip);
                            break;
                        }
                    }
                }
                GL.End();
            }

            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);
        }
    }
}
