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
using System.Text;
using System.Threading.Tasks;
using WF = System.Windows.Forms;

namespace Astrarium.Plugins.Horizon
{
    public class GroundRenderer : BaseRenderer
    {
        private readonly ISkyMap map;
        private readonly ISettings settings;
        private readonly ITextureManager textureManager;

        // TODO: move to settings!
        //private readonly Color colorGroundNight = Color.FromArgb(4, 10, 10);
        //private readonly Color colorGroundDay = Color.FromArgb(116, 185, 139);

        private readonly Lazy<TextRenderer> textRenderer = new Lazy<TextRenderer>(() => new TextRenderer(256, 32));

        private readonly string[] cardinalDirections = new string[] { "S", "SW", "W", "NW", "N", "NE", "E", "SE" };
        private readonly string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public GroundRenderer(ISkyMap map, ITextureManager textureManager, ISettings settings)
        {
            this.map = map;
            this.textureManager = textureManager;
            this.settings = settings;
        }

        public override RendererOrder Order => RendererOrder.Terrestrial;

        public override void Render(IMapContext map)
        {
            
        }

        public override void Render(ISkyMap map)
        {
            RenderHorizonLine();
            RenderGround();
            RenderCardinalLabels();
        }

        private void RenderHorizonLine()
        {
            if (!settings.Get<bool>("HorizonLine") || settings.Get("Ground")) return;

            var prj = map.SkyProjection;

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.LineSmooth);

            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Begin(PrimitiveType.LineLoop);
            GL.Color4(settings.Get<SkyColor>("ColorHorizon").Night);

            const int steps = 64;
            var hor = new CrdsHorizontal();
            for (int i = 0; i < steps; i++)
            {
                hor.Azimuth = (double)i / steps * 360;
                var p = prj.Project(hor);
                if (p != null)
                {
                    GL.Vertex2(p.X, p.Y);
                }
            }

            GL.End();

            GL.Disable(EnableCap.Blend);
        }

        private void RenderGround()
        {
            var prj = map.SkyProjection;
            if (!settings.Get<bool>("Ground")) return;

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // if only one of the flipping enabled
            if (prj.FlipVertical ^ prj.FlipHorizontal)
            {
                GL.CullFace(CullFaceMode.Back);
            }
            else
            {
                GL.CullFace(CullFaceMode.Front);
            }

            GL.BindTexture(TextureTarget.Texture2D, textureManager.GetTexture(Path.Combine(basePath, "Data", "pano.png"), fallbackPath: null, permanent: true));

            int steps = prj.Fov < 90 ? 32 : 128;

            // night texture dimming (default is 90%), clamping to range 0...100%
            decimal dimming = Math.Min(100, Math.Max(0, settings.Get<decimal>("GroundTextureNightDimming", 90)));

            // night color
            int nc = (int)((100 - dimming) / 100 * 255);

            // blackout coeff
            int c = nc + (int)((255 - nc) * map.DaylightFactor);

            if (settings.Get<ColorSchema>("Schema") == ColorSchema.Red)
            {
                // night vision tint
                GL.Color4(Color.FromArgb((int)(c * 0.6), 0, 0));
            }
            else
            {
                GL.Color4(Color.FromArgb(c, c, c));
            }

            for (double lat = -80; lat <= 90; lat += 10)
            {
                GL.Begin(PrimitiveType.TriangleStrip);

                for (int i = 0; i <= steps; i++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        var p = prj.Project(new CrdsHorizontal(i / (double)steps * 360, lat - k * 10));

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

        private void RenderCardinalLabels()
        {
            if (!settings.Get("LabelCardinalDirections")) return;

            var prj = map.SkyProjection;

            var fontMajor = settings.Get<Font>("CardinalDirectionsFont");
            var fontMinor = new Font(fontMajor.FontFamily, fontMajor.Size * 0.75f, fontMajor.Style);

            var color = settings.Get<SkyColor>("ColorCardinalDirections").Night;

            for (int i = 0; i < cardinalDirections.Length; i++)
            {
                if (prj.Fov > 90 && i % 2 == 1) continue;

                var p = prj.Project(new CrdsHorizontal((double)i / cardinalDirections.Length * 360, 0));
                if (prj.IsInsideScreen(p))
                {
                    string label = Text.Get($"CardinalDirections.{cardinalDirections[i]}");
                    var font = i % 2 == 0 ? fontMajor : fontMinor;
                    var size = WF.TextRenderer.MeasureText(label, font);
                    textRenderer.Value.DrawString(label, font, new SolidBrush(color), new Vec2(p.X - size.Width / 2, p.Y + size.Height / 2));
                }
            }
        }
    }
}
