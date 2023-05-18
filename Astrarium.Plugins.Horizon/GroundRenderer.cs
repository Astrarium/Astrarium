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

namespace Astrarium.Plugins.Horizon
{
    public class GroundRenderer : BaseRenderer
    {
        private readonly ISettings settings;
        private readonly ITextureManager textureManager;

        // TODO: move to settings!
        private readonly Color colorGroundNight = Color.FromArgb(4, 10, 10);
        private readonly Color colorGroundDay = Color.FromArgb(116, 185, 139);

        private readonly string[] cardinalDirections = new string[] { "S", "SW", "W", "NW", "N", "NE", "E", "SE" };

        private readonly string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public GroundRenderer(ITextureManager textureManager, ISettings settings)
        {
            this.textureManager = textureManager;
            this.settings = settings;
        }

        public override RendererOrder Order => RendererOrder.Terrestrial;

        public override void Render(IMapContext map)
        {
            if (settings.Get<bool>("Ground"))
            {
                const int POINTS_COUNT = 64;
                PointF[] hor = new PointF[POINTS_COUNT];
                double step = 2 * map.ViewAngle / (POINTS_COUNT - 1);
                SolidBrush brushGround = new SolidBrush(map.GetColor(colorGroundNight, colorGroundDay));

                // Bottom part of ground shape

                for (int i = 0; i < POINTS_COUNT; i++)
                {
                    var h = new CrdsHorizontal(map.Center.Azimuth - map.ViewAngle + step * i, 0);
                    hor[i] = map.Project(h);
                }

                if (hor.Any(h => !map.IsOutOfScreen(h)))
                {
                    GraphicsPath gp = new GraphicsPath();

                    gp.AddCurve(hor);

                    var pts = map.IsInverted ? 
                        new PointF[]
                    {
                        new PointF(map.Width + 1, -1),
                        new PointF(-1, -1),
                    } : new PointF[]
                    {
                        new PointF(map.Width + 1, map.Height + 1),
                        new PointF(-1, map.Height + 1)
                    };

                    if (hor.Last().X > map.Width / 2)
                    {
                        gp.AddLines(pts);
                    }
                    else
                    {
                        gp.AddLines(pts.Reverse().ToArray());
                    }

                    map.Graphics.FillPath(brushGround, gp);
                }
                else if (map.Center.Altitude <= 0)
                {
                    map.Graphics.FillRectangle(brushGround, 0, 0, map.Width, map.Height);
                }

                // Top part of ground shape 

                if (map.Center.Altitude > 0)
                {
                    for (int i = 0; i < POINTS_COUNT; i++)
                    {
                        var h = new CrdsHorizontal(map.Center.Azimuth - map.ViewAngle - step * i, 0);
                        hor[i] = map.Project(h);
                    }

                    if (hor.Count(h => !map.IsOutOfScreen(h)) > 2)
                    {
                        GraphicsPath gp = new GraphicsPath();

                        gp.AddCurve(hor);
                        gp.AddLines(new PointF[]
                        {
                            new PointF(map.Width + 1, -1),
                            new PointF(-1, -1),
                        });

                        map.Graphics.FillPath(brushGround, gp);
                    }
                }
            }

            if (map.Schema == ColorSchema.White || (!settings.Get<bool>("Ground") && settings.Get<bool>("HorizonLine")))
            {
                const int POINTS_COUNT = 64;
                PointF[] hor = new PointF[POINTS_COUNT];
                double step = 2 * map.ViewAngle / (POINTS_COUNT - 1);

                for (int i = 0; i < POINTS_COUNT; i++)
                {
                    var h = new CrdsHorizontal(map.Center.Azimuth - map.ViewAngle + step * i, 0);
                    hor[i] = map.Project(h);
                }

                if (hor.Any(h => !map.IsOutOfScreen(h)))
                {
                    Pen penHorizonLine = new Pen(map.GetColor("ColorHorizon"), 2);
                    map.Graphics.DrawCurve(penHorizonLine, hor);
                }
            }

            if (settings.Get<bool>("LabelCardinalDirections"))
            {
                Brush brushCardinalLabels = new SolidBrush(map.GetColor("ColorCardinalDirections"));
                StringFormat format = new StringFormat() { LineAlignment = StringAlignment.Near, Alignment = StringAlignment.Center };
                for (int i = 0; i < cardinalDirections.Length; i++)
                {
                    var h = new CrdsHorizontal(i * 360 / cardinalDirections.Length, 0);
                    if (Angle.Separation(h, map.Center) < map.ViewAngle)
                    {
                        PointF p = map.Project(h);
                        var fontBase = settings.Get<Font>("CardinalDirectionsFont");
                        var font = new Font(fontBase.FontFamily, fontBase.Size * (i % 2 == 0 ? 1 : 0.75f), fontBase.Style);

                        using (var gp = new GraphicsPath())
                        {
                            map.Graphics.DrawString(Text.Get($"CardinalDirections.{cardinalDirections[i]}"), font, brushCardinalLabels, p, format);
                        }
                    }
                }
            }
        }

        public override void Render(ISkyMap map)
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

            /*
            string texture;
            string fallbackTexture = "pano2-2k.png";
            if (prj.Fov > 45)
            {
                texture = "pano2-2k.png";
            }
            else if (prj.Fov > 20)
            {
                texture = "pano2-4k.png";
            }
            else
            {
                texture = "pano2-8k.png";
            }

            GL.BindTexture(TextureTarget.Texture2D, textureManager.GetTexture(texture, fallbackTexture));
            */
            GL.BindTexture(TextureTarget.Texture2D, textureManager.GetTexture(Path.Combine(basePath, "Data", "pano.png")));


            int steps = prj.Fov < 90 ? 32 : 128;

            // tint color
            int c = 10 + (int)((255 - 10) * map.DaylightFactor);

            if (settings.Get<ColorSchema>("Schema") == ColorSchema.Red)
            {
                GL.Color4(Color.FromArgb(c, 0, 0));
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
                            double s = 1 - (double)i / steps;
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


            string[] cardinals = new string[] { "S", "W", "N", "E" };

            // Cardinals

            var font = new Font(SystemFonts.DefaultFont.FontFamily, 14);

            for (int i = 0; i < cardinals.Length; i++)
            {
                var p = prj.Project(new CrdsHorizontal((double)i / cardinals.Length * 360, 0));
                if (prj.IsInsideScreen(p))
                {
                    
                    //var size = System.Windows.Forms.TextRenderer.MeasureText(cardinals[i], font);
                    //textRenderer.DrawString(cardinals[i], font, Brushes.DarkGreen, new PointF((int)(p.X - size.Width / 2), (int)(prj.ScreenHeight - p.Y - size.Height)), prj.ScreenWidth, prj.ScreenHeight);
                }
            }
        }
    }
}
