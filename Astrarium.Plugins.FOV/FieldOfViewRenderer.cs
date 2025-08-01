using Astrarium.Algorithms;
using Astrarium.Types;
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

        private readonly int[] xSigns = new int[] { -1, 1, 1, -1 };
        private readonly int[] ySigns = new int[] { 1, 1, -1, -1 };

        // TODO: move to settings
        private Font font = new Font("Arial", 8);
        public override RendererOrder Order => RendererOrder.Foreground;

        public FieldOfViewRenderer(ISkyMap map, ISettings settings)
        {
            this.map = map;
            this.settings = settings;
        }

        private CameraFovFrame selectedFrame = null;
        private float? selectedFrameRotation = null;
        private int selectedFrameCorner = 0;
        private Vec2 selectedFramePoint = null;

        public override void OnMouseDown(ISkyMap map, MouseButton mouseButton)
        {
            var prj = map.Projection;

            // detect whether mouse button has been pressed near one of Camera FOV frames
        
            if (mouseButton == MouseButton.Left)
            {
                bool isSelected = false;

                var frames = settings.Get("FovFrames", new List<FovFrame>())
                    .OfType<CameraFovFrame>().Where(x => x.Enabled && x is CameraFovFrame && NeedDrawCameraFrame(x))
                    .ToList();

                // mouse coordinates
                Vec2 m = (Vec2)map.MouseScreenCoordinates;

                // center of the map
                Vec2 c = new Vec2(map.Projection.ScreenWidth / 2, map.Projection.ScreenHeight / 2);

                foreach (var frame in frames)
                {
                    double rotAngle = GetCameraFrameRotation(frame);
                    float w2 = prj.GetDiskSize(frame.Width * 3600 / 2) /2 ;
                    float h2 = prj.GetDiskSize(frame.Height * 3600 / 2) / 2;

                    var mat = Mat4.ZRotation(Angle.ToRadians(rotAngle));

                    for (int i = 0; i < 4; i++)
                    {
                        Vec2 fp = c + mat * new Vec2(xSigns[i] * w2, ySigns[i] * h2);
                        if (m.Distance(fp) < 5)
                        {
                            selectedFrame = frame;
                            selectedFrameRotation = frame.Rotation;
                            selectedFramePoint = fp;
                            selectedFrameCorner = i;
                            isSelected = true;
                            break;
                        }
                    }
                }

                if (!isSelected)
                {
                    if (selectedFrame != null)
                    {
                        var allFrames = settings.Get("FovFrames", new List<FovFrame>());
                        settings.SetAndSave("FovFrames", allFrames);
                        ViewManager.ShowPopupMessage("$FovPlugin.OnScreenRotation.CompletedTooltip");
                        selectedFrame = null;
                    }
                }
                map.Invalidate();
            }
        }

        public override void OnMouseMove(ISkyMap map, MouseButton mouseButton)
        {
            var prj = map.Projection;

            // mouse coordinates
            var m = (Vec2)map.MouseScreenCoordinates;

            // map center
            Vec2 c = new Vec2(map.Projection.ScreenWidth / 2, map.Projection.ScreenHeight / 2);

            if (selectedFrame != null)
            {
                Vec2 b = m - c;
                Vec2 a = selectedFramePoint - c;

                double sign = (map.Projection.FlipHorizontal ^ map.Projection.FlipVertical ? 1 : -1);

                double theta = sign * Angle.ToDegrees(Math.Atan2(a.X * b.Y - a.Y * b.X, a.X * b.X + a.Y * b.Y));

                selectedFrame.Rotation = (float)Angle.To360(selectedFrameRotation.Value + theta);
                map.Invalidate();

                ViewManager.ShowPopupMessage("$FovPlugin.OnScreenRotation.RotatingTooltip");
            }
            else
            {
                var frames = settings.Get("FovFrames", new List<FovFrame>())
                                    .OfType<CameraFovFrame>().Where(x => x.Enabled && x is CameraFovFrame && NeedDrawCameraFrame(x))
                                    .ToList();

                bool showTooltip = false;

                foreach (var frame in frames)
                {
                    double rotAngle = GetCameraFrameRotation(frame);
                    float w2 = prj.GetDiskSize(frame.Width * 3600 / 2) / 2;
                    float h2 = prj.GetDiskSize(frame.Height * 3600 / 2) / 2;

                    var mat = Mat4.ZRotation(Angle.ToRadians(rotAngle));

                    for (int i = 0; i < 4; i++)
                    {
                        Vec2 fp = c + mat * new Vec2(xSigns[i] * w2, ySigns[i] * h2);
                        if (m.Distance(fp) < 5)
                        {
                            showTooltip = true;
                            break;
                        }
                    }
                }

                if (showTooltip)
                {
                    ViewManager.ShowTooltipMessage(m, "$FovPlugin.OnScreenRotation.CornerTooltip");
                }
            }
        }

        public override void Render(ISkyMap map)
        {
            var prj = map.Projection;
            var nightMode = settings.Get("NightMode");
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
                                var pen = new Pen(finderFrame.Color.Tint(nightMode));

                                var v1 = new Vec2(p.X, p.Y - diam / 2);
                                var v2 = new Vec2(p.X, p.Y + diam / 2);
                                GL.DrawLine(v1, v2, pen);

                                var h1 = new Vec2(p.X - diam / 2, p.Y);
                                var h2 = new Vec2(p.X + diam / 2, p.Y);
                                GL.DrawLine(h1, h2, pen);
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
                    float w2 = prj.GetDiskSize(cameraFrame.Width * 3600 / 2) / 2;
                    float h2 = prj.GetDiskSize(cameraFrame.Height * 3600 / 2) / 2;

                    if (NeedDrawCameraFrame(cameraFrame))
                    {
                        var color = frame.Color.Tint(nightMode);

                        var p = new PointF(prj.ScreenWidth / 2, prj.ScreenHeight / 2);
                        double rotAngle = GetCameraFrameRotation(cameraFrame);
                        
                        GL.PushMatrix();
                        GL.Translate(p.X, p.Y, 0);
                        GL.Rotate(rotAngle, 0, 0, 1);

                        GL.Enable(GL.BLEND);
                        GL.BlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);
                        GL.Enable(GL.LINE_SMOOTH);
                        GL.LineWidth(1);

                        GL.Begin(GL.LINE_LOOP);
                        GL.Color3(color);
                        for (int i = 0; i < 4; i++)
                        {
                            GL.Vertex2(xSigns[i] * w2, ySigns[i] *  h2);
                        }
                        GL.End();

                        // highlight selected corner
                        if (selectedFrame == frame)
                        {
                            GL.PointSize(10);
                            GL.Begin(GL.POINTS);
                            GL.Vertex2(xSigns[selectedFrameCorner] * w2, ySigns[selectedFrameCorner] * h2); 
                            GL.End();
                        }

                        DrawFrameLabel(frame, color, 2 * w2, 2 * h2);

                        GL.PopMatrix();

                    }
                }
            }
        }

        /// <summary>
        /// Detects whether Camera frame should be / can be drawn or not
        /// </summary>
        /// <param name="frame">Camera frame</param>
        /// <returns>
        /// True, if camera frame should be / can be drawn.
        /// </returns>
        private bool NeedDrawCameraFrame(CameraFovFrame frame)
        {
            var prj = map.Projection;
            float width = prj.GetDiskSize(frame.Width * 3600 / 2);
            float height = prj.GetDiskSize(frame.Height * 3600 / 2);
            return width > 5 && height > 5 && Math.Min(width, height) < Math.Sqrt(prj.ScreenWidth * prj.ScreenWidth + prj.ScreenHeight * prj.ScreenHeight);
        }

        /// <summary>
        /// Gets camera frame rotation relative to screen
        /// </summary>
        /// <param name="frame">Camera frame</param>
        /// <returns>Rotation angle, in degrees</returns>
        private double GetCameraFrameRotation(CameraFovFrame frame)
        {
            var prj = map.Projection;
            double rotAngle = 0;

            if (frame.RotateOrigin == FovFrameRotateOrigin.Equatorial)
            {
                if (prj.ViewMode == ProjectionViewType.Horizontal)
                {
                    rotAngle = prj.GetAxisRotation(prj.CenterEquatorial, -frame.Rotation);
                }
                else if (prj.ViewMode == ProjectionViewType.Equatorial)
                {
                    rotAngle = -frame.Rotation;
                }
            }
            else if (frame.RotateOrigin == FovFrameRotateOrigin.Horizontal)
            {
                if (prj.ViewMode == ProjectionViewType.Horizontal)
                {
                    rotAngle = -frame.Rotation;
                }
                else if (prj.ViewMode == ProjectionViewType.Equatorial)
                {
                    rotAngle = prj.GetAxisRotation(prj.CenterHorizontal, -frame.Rotation);
                }
            }

            return rotAngle;
        }

        private void DrawFrameLabel(FovFrame frame, Color color, float width, float height)
        {
            var labelSize = WF.TextRenderer.MeasureText(frame.Label, font, Size.Empty);
            if (labelSize.Width <= width * 2)
            {
                var brush = new SolidBrush(color);
                GL.DrawString(frame.Label, font, brush, new PointF(-labelSize.Width / 2, -height / 2 - labelSize.Height / 2), antiAlias: true);
            }
        }

        private void DrawFovCircle(float frameSize, short shading, FovFrame frame, bool isOuter)
        {
            var prj = map.Projection;
            var nightMode = settings.Get("NightMode");
            float radius = frameSize * 3600 / 2;
            float size = prj.GetDiskSize(radius);
            var color = frame.Color.Tint(nightMode);

            // do not draw frame if its size exceeds screen bounds
            if (size > 10 && size < Math.Sqrt(prj.ScreenWidth * prj.ScreenWidth + prj.ScreenHeight * prj.ScreenHeight))
            {
                if (isOuter && shading > 0)
                {
                    double factor = Math.Min(1, frameSize / (prj.Fov / 2));
                    short shadePercentage = (short)(factor * shading);
                    DrawShading(shadePercentage, size);
                }

                var p = new Vec2(prj.ScreenWidth / 2, prj.ScreenHeight / 2);

                GL.PushMatrix();
                GL.Translate(p.X, p.Y, 0);

                GL.DrawEllipse(new Vec2(), new Pen(color), size / 2);

                if (isOuter)
                {
                    DrawFrameLabel(frame, color, size, size);
                }

                GL.PopMatrix();
            }
        }

        private void DrawShading(short shading, float size)
        {
            var prj = map.Projection;
            int w = prj.ScreenWidth;
            int h = prj.ScreenHeight;
            const int gap = 5;

            GL.Enable(GL.BLEND);
            GL.BlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);
            GL.Enable(GL.STENCIL_TEST);

            GL.ClearStencil(0);
            GL.Clear(GL.STENCIL_BUFFER_BIT);

            GL.ColorMask(false, false, false, false);
            GL.DepthMask(false);
            GL.StencilFunc(GL.ALWAYS, 1, 0xFF);
            GL.StencilOp(GL.KEEP, GL.KEEP, GL.REPLACE);

            GL.Begin(GL.TRIANGLE_FAN);
            for (int i = 0; i <= 64; i++)
            {
                double ang = i / 64.0 * 2 * Math.PI;
                Vec2 v = new Vec2(w / 2 + size / 2 * Math.Cos(ang), h / 2 + size / 2 * Math.Sin(ang));
                GL.Vertex2(v.X, v.Y);
            }
            GL.End();

            GL.ColorMask(true, true, true, true);
            GL.DepthMask(true);
            GL.StencilFunc(GL.NOTEQUAL, 1, 0xFF);
            GL.StencilOp(GL.KEEP, GL.KEEP, GL.KEEP);

            GL.Begin(GL.LINE_LOOP);
            GL.Vertex2(-gap, -gap);
            GL.Vertex2(-gap, h + gap);
            GL.Vertex2(w + gap, h + gap);
            GL.Vertex2(w + gap, -gap);
            GL.End();

            GL.Begin(GL.QUADS);
            GL.Color4(Color.FromArgb((byte)(shading / 100f * 255), 0, 0, 0));
            GL.Vertex2(-gap, -gap);
            GL.Vertex2(-gap, h + gap);
            GL.Vertex2(w + gap, h + gap);
            GL.Vertex2(w + gap, -gap);
            GL.End();

            GL.Disable(GL.BLEND);
            GL.Disable(GL.STENCIL_TEST);
        }
    }
}
