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
        private readonly ILandscapesManager landscapesManager;
        private readonly ITextureManager textureManager;

        private readonly Lazy<TextRenderer> textRenderer = new Lazy<TextRenderer>(() => new TextRenderer(256, 32));

        private readonly string[] cardinalDirections = new string[] { "S", "SW", "W", "NW", "N", "NE", "E", "SE" };
        private readonly string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public GroundRenderer(ISkyMap map, ILandscapesManager landscapesManager, ITextureManager textureManager, ISettings settings)
        {
            this.map = map;
            this.landscapesManager = landscapesManager;
            this.textureManager = textureManager;
            this.settings = settings;
        }

        public override RendererOrder Order => RendererOrder.Terrestrial;

        public override void Render(ISkyMap map)
        {
            RenderHorizonLine();
            RenderGround();
            RenderCardinalLabels();
        }

        private void RenderHorizonLine()
        {
            if (!settings.Get<bool>("HorizonLine") || settings.Get("Ground")) return;

            var prj = map.Projection;
            var nightMode = settings.Get("NightMode");

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.LineSmooth);
            GL.LineWidth(5);

            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Begin(PrimitiveType.LineStrip);
            GL.Color4(settings.Get<Color>("ColorHorizon").Tint(nightMode));

            const int steps = 64;
            var hor = new CrdsHorizontal();
            for (int i = 0; i <= steps; i++)
            {
                hor.Azimuth = (double)i / steps * 360;
                var p = prj.Project(hor);
                if (p != null)
                {
                    GL.Vertex2(p.X, p.Y);
                }
                else
                {
                    GL.End();
                    GL.Begin(PrimitiveType.LineStrip);
                }
            }

            GL.End();

            GL.LineWidth(1);
            GL.Disable(EnableCap.Blend);
        }

        private void RenderGround()
        {
            var prj = map.Projection;
            if (!settings.Get<bool>("Ground")) return;

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

            string landscapeName = settings.Get<string>("Landscape");
            Landscape landscape = landscapesManager.Landscapes.FirstOrDefault(x => x.Title == landscapeName);
            if (!File.Exists(landscape?.Path)) return;

            string landscapeFileName = Path.GetFileNameWithoutExtension(landscape.Path);
            string landscapeLocation = Directory.GetParent(landscape.Path).FullName;
            string fallbackPath = Path.Combine(landscapeLocation, $"{landscapeFileName}.thumb");
            fallbackPath = File.Exists(fallbackPath) ? fallbackPath : null;

            int textureId = textureManager.GetTexture(landscape.Path, fallbackPath, permanent: true, action: null, alphaChannel: true);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            int steps = prj.Fov < 90 ? 32 : 128;

            // night texture dimming (default is 90%), clamping to range 0...100%
            decimal dimming = Math.Min(100, Math.Max(0, settings.Get<decimal>("GroundTextureNightDimming", 90)));

            // night color
            int nc = (int)((100 - dimming) / 100 * 255);

            // blackout coeff
            int c = nc + (int)((255 - nc) * map.DaylightFactor);

            if (settings.Get("NightMode"))
            {
                // night vision tint
                GL.Color4(Color.FromArgb((int)(c * 0.6), 0, 0));
            }
            else
            {
                if (textureId > 0)
                    GL.Color4(Color.FromArgb(c, c, c));
                else
                    GL.Color4(Color.FromArgb(0, (int)(c * 0.6), 0));
            }

            double latStop = textureId > 0 ? 90 : 0;

            for (double lat = -80; lat <= latStop; lat += 10)
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

            var prj = map.Projection;

            var nightMode = settings.Get("NightMode");
            var fontMajor = settings.Get<Font>("CardinalDirectionsFont");
            var fontMinor = new Font(fontMajor.FontFamily, fontMajor.Size * 0.75f, fontMajor.Style);
            var color = settings.Get<Color>("ColorCardinalDirections").Tint(nightMode);
            var brush = new SolidBrush(color);

            for (int i = 0; i < cardinalDirections.Length; i++)
            {
                if (prj.Fov > 90 && i % 2 == 1) continue;

                var p = prj.Project(new CrdsHorizontal((double)i / cardinalDirections.Length * 360, 0));
                if (prj.IsInsideScreen(p))
                {
                    string label = Text.Get($"CardinalDirections.{cardinalDirections[i]}");
                    var font = i % 2 == 0 ? fontMajor : fontMinor;
                    var size = WF.TextRenderer.MeasureText(label, font);
                    textRenderer.Value.DrawString(label, font, brush, new Vec2(p.X - size.Width / 2, p.Y + size.Height / 2));
                }
            }
        }
    }
}
