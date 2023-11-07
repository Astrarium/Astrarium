using Astrarium.Types;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WF = System.Windows.Forms;

namespace Astrarium.Plugins.FOV
{
    public class FieldOfViewRenderer : BaseRenderer
    {
        private readonly ISkyMap map;
        private readonly ISettings settings;
        private readonly Lazy<TextRenderer> textRenderer = new Lazy<TextRenderer>(() => new TextRenderer(256, 32));

        private Font font = new Font("Arial", 8);
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
                        DrawFovCircle(size, finderFrame.Shading, finderFrame, isOuter: outer);

                        if (finderFrame.Crosslines && outer)
                        {
                            float radius = size * 3600 / 2;
                            float diam = prj.GetDiskSize(radius);

                            // do not draw frame if its size exceeds screen bounds
                            if (diam > 10 && diam < Math.Sqrt(prj.ScreenWidth * prj.ScreenWidth + prj.ScreenHeight * prj.ScreenHeight))
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
                    DrawFovCircle(circularFrame.Size, circularFrame.Shading, circularFrame, isOuter: true);
                }
                else if (frame is CameraFovFrame cameraFrame)
                {
                    float width = prj.GetDiskSize(cameraFrame.Width * 3600 / 2);
                    float height = prj.GetDiskSize(cameraFrame.Height * 3600 / 2);

                    // do not draw frame if its size exceeds screen bounds
                    if (width > 5 && height > 5 && Math.Min(width, height) < Math.Sqrt(prj.ScreenWidth * prj.ScreenWidth + prj.ScreenHeight * prj.ScreenHeight))
                    {
                        var color = frame.Color.Night.Tint(schema);

                        var p = new PointF(prj.ScreenWidth / 2, prj.ScreenHeight / 2);
                        double rotAngle = 0;
                        if (cameraFrame.RotateOrigin == FovFrameRotateOrigin.Equatorial)
                        {
                            if (prj.ViewMode == ProjectionViewType.Horizontal)
                            {
                                rotAngle = prj.GetAxisRotation(prj.CenterEquatorial, cameraFrame.Rotation);
                            }
                            else if (prj.ViewMode == ProjectionViewType.Equatorial)
                            {
                                rotAngle = cameraFrame.Rotation;
                            }
                        }
                        else if (cameraFrame.RotateOrigin == FovFrameRotateOrigin.Horizontal)
                        {
                            if (prj.ViewMode == ProjectionViewType.Horizontal)
                            {
                                rotAngle = cameraFrame.Rotation;
                            }
                            else if (prj.ViewMode == ProjectionViewType.Equatorial)
                            {
                                rotAngle = prj.GetAxisRotation(prj.CenterHorizontal, cameraFrame.Rotation);
                            }
                        }

                        GL.PushMatrix();
                        GL.Translate(p.X, p.Y, 0);
                        GL.Rotate(rotAngle, 0, 0, 1);

                        GL.Enable(EnableCap.Blend);
                        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                        GL.Enable(EnableCap.LineSmooth);
                        GL.LineWidth(1);

                        GL.Begin(PrimitiveType.LineLoop);
                        GL.Color3(color);
                        GL.Vertex2(-width / 2, height / 2);
                        GL.Vertex2(width / 2, height / 2);
                        GL.Vertex2(width / 2, -height / 2);
                        GL.Vertex2(-width / 2, -height / 2);
                        GL.End();

                        DrawFrameLabel(frame, color, width, height);

                        GL.PopMatrix();
                    }
                }
            }
        }

        public override void Render(IMapContext map) { }

        private void DrawFrameLabel(FovFrame frame, Color color, float width, float height)
        {
            var labelSize = WF.TextRenderer.MeasureText(frame.Label, font, Size.Empty);
            if (labelSize.Width <= width * 2)
            {
                var brush = new SolidBrush(color);
                textRenderer.Value.DrawString(frame.Label, font, brush, new PointF((float)(-labelSize.Width / 2), (float)(-height / 2 - labelSize.Height / 2)));
            }
        }

        private void DrawFovCircle(float frameSize, short shading, FovFrame frame, bool isOuter)
        {
            var prj = map.SkyProjection;
            var schema = settings.Get<ColorSchema>("Schema");
            float radius = frameSize * 3600 / 2;
            float size = prj.GetDiskSize(radius);
            var color = frame.Color.Night.Tint(schema);

            // do not draw frame if its size exceeds screen bounds
            if (size > 10 && size < Math.Sqrt(prj.ScreenWidth * prj.ScreenWidth + prj.ScreenHeight * prj.ScreenHeight))
            {
                if (isOuter && shading > 0 && frameSize >= prj.Fov / 2)
                {
                    DrawShading(shading, size);
                }

                var p = new Vec2(prj.ScreenWidth / 2, prj.ScreenHeight / 2);

                GL.PushMatrix();
                GL.Translate(p.X, p.Y, 0);

                Primitives.DrawEllipse(new Vec2(), new Pen(color), size / 2);

                if (isOuter)
                {
                    DrawFrameLabel(frame, color, size, size);
                }

                GL.PopMatrix();
            }
        }

        private void DrawShading(short shading, float size)
        {
            var prj = map.SkyProjection;
            int w = prj.ScreenWidth;
            int h = prj.ScreenHeight;
            const int gap = 5;

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.StencilTest);

            GL.ClearStencil(0);
            GL.Clear(ClearBufferMask.StencilBufferBit);

            GL.ColorMask(false, false, false, false);
            GL.DepthMask(false);
            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);

            GL.Begin(PrimitiveType.TriangleFan);
            for (int i = 0; i <= 64; i++)
            {
                double ang = i / 64.0 * 2 * Math.PI;
                Vec2 v = new Vec2(w / 2 + size / 2 * Math.Cos(ang), h / 2 + size / 2 * Math.Sin(ang));
                GL.Vertex2(v.X, v.Y);
            }
            GL.End();

            GL.ColorMask(true, true, true, true);
            GL.DepthMask(true);
            GL.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);

            GL.Begin(PrimitiveType.LineLoop);
            GL.Vertex2(-gap, -gap);
            GL.Vertex2(-gap, h + gap);
            GL.Vertex2(w + gap, h + gap);
            GL.Vertex2(w + gap, -gap);
            GL.End();

            GL.Begin(PrimitiveType.Quads);
            GL.Color4(Color.FromArgb((byte)(shading / 100f * 255), 0, 0, 0));
            GL.Vertex2(-gap, -gap);
            GL.Vertex2(-gap, h + gap);
            GL.Vertex2(w + gap, h + gap);
            GL.Vertex2(w + gap, -gap);
            GL.End();

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.StencilTest);
        }
    }
}
