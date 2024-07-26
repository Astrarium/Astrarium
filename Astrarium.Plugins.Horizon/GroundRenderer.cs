using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using WF = System.Windows.Forms;

namespace Astrarium.Plugins.Horizon
{
    public class GroundRenderer : BaseRenderer
    {
        private readonly ISkyMap map;
        private readonly ISettings settings;
        private readonly ILandscapesManager landscapesManager;

        private readonly string[] cardinalDirections = new string[] { "S", "SW", "W", "NW", "N", "NE", "E", "SE" };

        public GroundRenderer(ISkyMap map, ILandscapesManager landscapesManager, ISettings settings)
        {
            this.map = map;
            this.landscapesManager = landscapesManager;
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

            GL.Enable(GL.BLEND);
            GL.BlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);
            GL.Enable(GL.LINE_SMOOTH);
            GL.LineWidth(5);

            GL.Hint(GL.LINE_SMOOTH_HINT, GL.NICEST);
            GL.Begin(GL.LINE_STRIP);
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
                    GL.Begin(GL.LINE_STRIP);
                }
            }

            GL.End();

            GL.LineWidth(1);
            GL.Disable(GL.BLEND);
        }

        private void RenderGround()
        {
            var prj = map.Projection;
            if (!settings.Get<bool>("Ground")) return;
            int textureId = 0;
            double aziShift = 0;
            var landmarks = new Landmark[0];

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

            if (settings.Get("UseLandscape"))
            {
                string landscapeName = settings.Get<string>("Landscape");
                Landscape landscape = landscapesManager.Landscapes.FirstOrDefault(x => x.Title == landscapeName);
                if (File.Exists(landscape?.Path))
                {
                    string landscapeFileName = Path.GetFileNameWithoutExtension(landscape.Path);
                    string landscapeLocation = Directory.GetParent(landscape.Path).FullName;
                    aziShift = landscape.AzimuthShift;
                    landmarks = landscape.Landmarks;
                    textureId = GL.GetTexture(landscape.Path, permanent: true, readyCallback: map.Invalidate);
                    GL.BindTexture(GL.TEXTURE_2D, textureId);
                }
            }
            else
            {
                GL.Disable(GL.TEXTURE_2D);
            }

            int steps = prj.Fov < 90 ? 32 : 128;

            // night texture dimming (default is 90%), clamping to range 0...100%
            decimal dimming = Math.Min(100, Math.Max(0, settings.Get<decimal>("GroundTextureNightDimming", 90)));

            // night color
            int nc = (int)((100 - dimming) / 100 * 255);

            // blackout coeff
            int c = nc + (int)((255 - nc) * map.DaylightFactor);

            // night mode flag
            bool nightMode = settings.Get("NightMode");

            // ground transparency as alpha byte
            int groundTransparency = 255 - (int)(settings.Get<decimal>("GroundTransparency") / 100 * 255);

            if (nightMode)
            {
                // night vision tint
                GL.Color4(Color.FromArgb(groundTransparency, (int)(c * 0.6), 0, 0));
            }
            else
            {
                if (textureId > 0)
                    GL.Color4(Color.FromArgb(groundTransparency, c, c, c));
                else
                    GL.Color4(Color.FromArgb(groundTransparency, 0, (int)(c * 0.6), 0));
            }

            double latStop = textureId > 0 ? 90 : 0;

            for (double lat = -80; lat <= latStop; lat += 10)
            {
                GL.Begin(GL.TRIANGLE_STRIP);

                for (int i = 0; i <= steps; i++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        var p = prj.Project(new CrdsHorizontal(i / (double)steps * 360 + aziShift, lat - k * 10));

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

            if (landmarks != null && settings.Get("Landmarks"))
            {
                foreach (var landmark in landmarks)
                {
                    if (landmark.FOV == null || prj.Fov <= landmark.FOV.Value)
                    {
                        var color = (landmark.Color ?? Color.Black).Tint(nightMode);

                        // starting point
                        var p = prj.Project(new CrdsHorizontal(landmark.Azimuth, landmark.Altitude));

                        // wide object landmark
                        if (landmark.Width != null)
                        {
                            var pen = new Pen(color);
                            for (int i = 0; i < 12; i++)
                            {
                                var p0 = prj.Project(new CrdsHorizontal(landmark.Azimuth + i / 12.0 * landmark.Width.Value, landmark.Altitude));
                                var p1 = prj.Project(new CrdsHorizontal(landmark.Azimuth + (i + 1) / 12.0 * landmark.Width.Value, landmark.Altitude));

                                if (p0 != null && p1 != null)
                                {
                                    GL.DrawLine(p0, p1, pen);
                                }
                            }

                            // central point
                            var pc = prj.Project(new CrdsHorizontal(landmark.Azimuth + landmark.Width.Value / 2, landmark.Altitude));

                            if (prj.IsInsideScreen(pc))
                            {
                                GL.DrawString($"{landmark.Label} ", System.Drawing.SystemFonts.DefaultFont, new SolidBrush(color), new Vec2(pc.X, pc.Y + 10), verticalAlign: StringAlignment.Center, horizontalAlign: StringAlignment.Center, antiAlias: true);
                            } 
                        }
                        // single-point landmark
                        else
                        {
                            if (prj.IsInsideScreen(p))
                            {
                                var p1 = new Vec2(p.X, p.Y + 30);
                                var p0 = new Vec2(p.X + 5, p.Y + 30);
                                GL.DrawLine(p1, p, new Pen(color));
                                GL.DrawString($"{landmark.Label} ", System.Drawing.SystemFonts.DefaultFont, new SolidBrush(color), p0, antiAlias: true);
                            }
                        }
                    }
                }
            }
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
                    GL.DrawString(label, font, brush, new Vec2(p.X - size.Width / 2, p.Y + size.Height / 2));
                }
            }
        }
    }
}
