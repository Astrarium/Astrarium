using Astrarium.Algorithms;
using Astrarium.Types;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF = System.Windows.Forms;

namespace Astrarium.Plugins.FOV
{
    public class FieldOfViewRenderer : BaseRenderer
    {
        private readonly ISkyMap map;
        private readonly ISettings settings;
        private readonly Lazy<TextRenderer> textRenderer = new Lazy<TextRenderer>(() => new TextRenderer(256, 32));

        private Font font = new Font("Arial", 8);
        private StringFormat format = new StringFormat() { Alignment = StringAlignment.Center };

        public override RendererOrder Order => RendererOrder.Foreground;

        public FieldOfViewRenderer(ISkyMap map, ISettings settings)
        {
            this.map = map;
            this.settings = settings;
        }

        public override void Render(ISkyMap map)
        {
            var prj = map.SkyProjection;
            var schema = settings.Get<ColorSchema>("Schema");
            var frames = settings.Get<List<FovFrame>>("FovFrames").Where(f => f.Enabled);
            foreach (var frame in frames)
            {
                if (frame is FinderFovFrame finderFrame)
                {
                    var sizes = finderFrame.Sizes.OrderByDescending(s => s);
                    bool outer = true;
                    foreach (var size in sizes)
                    {
                        DrawFovCircle(size, finderFrame, isOuter: outer);

                        if (finderFrame.Crosslines && outer)
                        {
                            float radius = size * 3600 / 2;
                            float diam = prj.GetDiskSize(radius);

                            // do not draw frame if its size exceeds screen bounds
                            if (diam < Math.Sqrt(prj.ScreenWidth * prj.ScreenWidth + prj.ScreenHeight * prj.ScreenHeight))
                            {
                                var p = new Vec2(prj.ScreenWidth / 2, prj.ScreenHeight / 2);
                                var pen = new Pen(finderFrame.Color.Night.Tint(schema));
                                
                                var v1 = new Vec2(p.X, p.Y - diam / 2);
                                var v2 = new Vec2(p.X, p.Y + diam / 2);
                                Primitives.DrawLine(v1, v2, pen);

                                var h1 = new Vec2(p.X - diam / 2, p.Y);
                                var h2 = new Vec2(p.X + diam / 2, p.Y);
                                Primitives.DrawLine(h1, h2, pen);
                            }
                        }

                        outer = false;
                    }
                }
                else if (frame is CircularFovFrame circularFrame)
                {
                    DrawFovCircle(circularFrame.Size, circularFrame, isOuter: true);
                }
                else if (frame is CameraFovFrame cameraFrame)
                {
                    float width = prj.GetDiskSize(cameraFrame.Width * 3600 / 2);
                    float height = prj.GetDiskSize(cameraFrame.Height * 3600 / 2);

                    // do not draw frame if its size exceeds screen bounds
                    if (Math.Min(width, height) < Math.Sqrt(prj.ScreenWidth * prj.ScreenWidth + prj.ScreenHeight * prj.ScreenHeight))
                    {
                        var pCenter = new PointF(prj.ScreenWidth / 2, prj.ScreenHeight / 2);
                        double rotAngle = 0;
                        if (cameraFrame.RotateOrigin == FovFrameRotateOrigin.Equatorial)
                        {
                            rotAngle = prj.GetAxisRotation(prj.CenterEquatorial, cameraFrame.Rotation);
                        }
                        else if (cameraFrame.RotateOrigin == FovFrameRotateOrigin.Horizontal)
                        {
                            rotAngle = cameraFrame.Rotation;
                        }

                        

                        //if (frame.Shading > 0 && cameraFrame.Height >= map.ViewAngle / 2)
                        //{
                        //    var rect = new GraphicsPath();
                        //    rect.AddRectangle(new RectangleF(-width / 2, -height / 2, width, height));

                        //    var shading = new Region(new RectangleF(0, 0, map.Width, map.Height));
                        //    shading.Exclude(rect);

                        //    int transparency = (int)(frame.Shading / 100f * 255);
                        //    var solidBrush = new SolidBrush(Color.FromArgb(transparency, map.GetSkyColor()));
                        //    map.Graphics.FillRegion(solidBrush, shading);
                        //}

                        // draw frame
                        //map.Graphics.DrawRectangle(new Pen(frame.Color.GetColor(map.Schema, map.DayLightFactor)), -width / 2, -height / 2, width, height);

                        GL.Enable(EnableCap.Blend);
                        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                        GL.Enable(EnableCap.LineSmooth);
                        GL.LineWidth(1);

                        GL.PushMatrix();
                        GL.Translate(pCenter.X, pCenter.Y, 0);
                        GL.Rotate(rotAngle, 0, 0, 1);

                        GL.Begin(PrimitiveType.LineLoop);

                        GL.Color3(frame.Color.Night.Tint(schema));

                        GL.Vertex2(-width / 2, height / 2);
                        GL.Vertex2(width / 2, height / 2);
                        GL.Vertex2(width / 2, -height / 2);
                        GL.Vertex2(-width / 2, -height / 2);

                        GL.End();

                        GL.PopMatrix();


                        
                        //float labelWidth = map.Graphics.MeasureString(frame.Label, font).Width;
                        //if (labelWidth <= width * 2)
                        //{
                        //    map.Graphics.DrawString(frame.Label, font, new SolidBrush(frame.Color.GetColor(map.Schema, map.DayLightFactor)), new PointF(0, height / 2), format);
                        //}

                        //map.Graphics.ResetTransform();
                    }
                }
            }

        }

        public override void Render(IMapContext map)
        {
            var frames = settings.Get<List<FovFrame>>("FovFrames").Where(f => f.Enabled);

            foreach (var frame in frames)
            {
                if (frame is FinderFovFrame finderFrame)
                {
                    var sizes = finderFrame.Sizes.OrderByDescending(s => s);
                    bool outer = true;
                    foreach (var size in sizes)
                    {
                        DrawFovCircle(map, size, finderFrame, isOuter: outer);

                        if (finderFrame.Crosslines)
                        {
                            float radius = size * 3600 / 4;
                            float diam = map.GetDiskSize(radius);

                            // do not draw frame if its size exceeds screen bounds
                            if (diam < Math.Sqrt(map.Width * map.Width + map.Height * map.Height))
                            {
                                map.Graphics.DrawPlusCross(new Pen(frame.Color.GetColor(map.Schema, map.DayLightFactor)), new PointF(map.Width / 2, map.Height / 2), diam);
                            }
                        }

                        outer = false;
                    }
                }
                else if (frame is CircularFovFrame circularFrame)
                {
                    DrawFovCircle(map, circularFrame.Size, circularFrame, isOuter: true);
                }
                else if (frame is CameraFovFrame cameraFrame)
                {
                    float width = map.GetDiskSize(cameraFrame.Width * 3600 / 2);
                    float height = map.GetDiskSize(cameraFrame.Height * 3600 / 2);

                    // do not draw frame if its size exceeds screen bounds
                    if (Math.Min(width, height) < Math.Sqrt(map.Width * map.Width + map.Height * map.Height))
                    {
                        var pCenter = new PointF(map.Width / 2, map.Height / 2);
                        if (cameraFrame.RotateOrigin == FovFrameRotateOrigin.Equatorial)
                        {
                            map.Rotate(pCenter, map.Center.ToEquatorial(map.GeoLocation, map.SiderealTime), cameraFrame.Rotation);
                        }
                        else if (cameraFrame.RotateOrigin == FovFrameRotateOrigin.Horizontal)
                        {
                            map.Rotate(pCenter, cameraFrame.Rotation);
                        }

                        if (frame.Shading > 0 && cameraFrame.Height >= map.ViewAngle / 2)
                        {
                            var rect = new GraphicsPath();
                            rect.AddRectangle(new RectangleF(-width / 2, -height / 2, width, height));

                            var shading = new Region(new RectangleF(0, 0, map.Width, map.Height));
                            shading.Exclude(rect);

                            int transparency = (int)(frame.Shading / 100f * 255);
                            var solidBrush = new SolidBrush(Color.FromArgb(transparency, map.GetSkyColor()));
                            map.Graphics.FillRegion(solidBrush, shading);
                        }

                        map.Graphics.DrawRectangle(new Pen(frame.Color.GetColor(map.Schema, map.DayLightFactor)), -width / 2, -height / 2, width, height);
                        float labelWidth = map.Graphics.MeasureString(frame.Label, font).Width;
                        if (labelWidth <= width * 2)
                        {
                            map.Graphics.DrawString(frame.Label, font, new SolidBrush(frame.Color.GetColor(map.Schema, map.DayLightFactor)), new PointF(0, height / 2), format);
                        }

                        map.Graphics.ResetTransform();
                    }
                }
            }
        }

        private void DrawFovCircle(float frameSize, FovFrame frame, bool isOuter)
        {
            var prj = map.SkyProjection;
            var schema = settings.Get<ColorSchema>("Schema");
            float radius = frameSize * 3600 / 2;
            float size = prj.GetDiskSize(radius);

            // do not draw frame if its size exceeds screen bounds
            if (size < Math.Sqrt(prj.ScreenWidth * prj.ScreenWidth + prj.ScreenHeight * prj.ScreenHeight))
            {
                if (isOuter && frame.Shading > 0 && frameSize >= prj.Fov / 2)
                {
                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                    GL.Enable(EnableCap.StencilTest);

                    GL.ClearStencil(0);
                    GL.Clear(ClearBufferMask.StencilBufferBit);

                    GL.ColorMask(false, false, false, false);
                    GL.DepthMask(false);
                    GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);

                    // frame outline
                    GL.PushMatrix();
                    GL.Translate(prj.ScreenWidth / 2, prj.ScreenHeight / 2, 0);
                    GL.Begin(PrimitiveType.TriangleFan);
                    for (int i = 0; i <= 64; i++)
                    {
                        double ang = i / 64.0 * 2 * Math.PI;
                        Vec2 v = new Vec2(size / 2 * Math.Cos(ang), size / 2 * Math.Sin(ang));
                        GL.Vertex2(v.X, v.Y);
                    }
                    GL.End();
                    GL.PopMatrix();

                    // Draw rectangle, masking out fragments with 1's in the stencil buffer
                    GL.ColorMask(true, true, true, true);
                    GL.DepthMask(true);
                    GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
                    GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);

                    GL.Begin(PrimitiveType.LineLoop);
                    GL.Vertex2(0, 0);
                    GL.Vertex2(0, prj.ScreenHeight);
                    GL.Vertex2(prj.ScreenWidth, prj.ScreenHeight);
                    GL.Vertex2(prj.ScreenWidth, 0);
                    GL.End();

                    GL.Begin(PrimitiveType.Quads);
                    GL.Color4(Color.FromArgb((byte)(frame.Shading / 100f * 255), 0, 0, 0));
                    GL.Vertex2(0, 0);
                    GL.Vertex2(0, prj.ScreenHeight);
                    GL.Vertex2(prj.ScreenWidth, prj.ScreenHeight);
                    GL.Vertex2(prj.ScreenWidth, 0);
                    GL.End();

                    GL.Disable(EnableCap.Blend);
                    GL.Disable(EnableCap.StencilTest);
                }

                var p = new Vec2(prj.ScreenWidth / 2, prj.ScreenHeight / 2);
                Primitives.DrawEllipse(p, new Pen(frame.Color.Night.Tint(schema)), size / 2);

                if (isOuter)
                {
                    var labelSize = WF.TextRenderer.MeasureText(frame.Label, font, Size.Empty);
                    if (labelSize.Width <= size * 2)
                    {
                        var brush = new SolidBrush(frame.Color.Night.Tint(schema));
                        textRenderer.Value.DrawString(frame.Label, font, brush, new PointF( (float)(p.X - labelSize.Width / 2), (float)(p.Y - size / 2 - labelSize.Height / 2)));
                    }
                }
            }
        }

        private void DrawFovCircle(IMapContext map, float frameSize, FovFrame frame, bool isOuter)
        {
            float radius = frameSize * 3600 / 2;
            float size = map.GetDiskSize(radius);

            // do not draw frame if its size exceeds screen bounds
            if (size < Math.Sqrt(map.Width * map.Width + map.Height * map.Height))
            {
                if (isOuter && frame.Shading > 0 && frameSize >= map.ViewAngle / 2)
                {
                    var circle = new GraphicsPath();
                    circle.AddEllipse(map.Width / 2 - size / 2, map.Height / 2 - size / 2, size, size);

                    var shading = new Region(new RectangleF(0, 0, map.Width, map.Height));
                    shading.Exclude(circle);

                    int transparency = (int)(frame.Shading / 100f * 255);
                    var solidBrush = new SolidBrush(Color.FromArgb(transparency, map.GetSkyColor()));
                    map.Graphics.FillRegion(solidBrush, shading);
                }
                map.Graphics.DrawEllipse(new Pen(frame.Color.GetColor(map.Schema, map.DayLightFactor)), map.Width / 2 - size / 2, map.Height / 2 - size / 2, size, size);

                if (isOuter)
                {
                    float labelWidth = map.Graphics.MeasureString(frame.Label, font).Width;
                    if (labelWidth <= size * 2)
                    {
                        map.Graphics.DrawString(frame.Label, font, new SolidBrush(frame.Color.GetColor(map.Schema, map.DayLightFactor)), new PointF(map.Width / 2, map.Height / 2 + size / 2), format);
                    }
                }
            }
        }
    }
}
