﻿using Astrarium.Types;
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

        // TODO: move to settings
        private Font font = new Font("Arial", 8);
        public override RendererOrder Order => RendererOrder.Foreground;

        public FieldOfViewRenderer(ISkyMap map, ISettings settings)
        {
            this.map = map;
            this.settings = settings;
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
                    float width = prj.GetDiskSize(cameraFrame.Width * 3600 / 2);
                    float height = prj.GetDiskSize(cameraFrame.Height * 3600 / 2);

                    // do not draw frame if its size exceeds screen bounds
                    if (width > 5 && height > 5 && Math.Min(width, height) < Math.Sqrt(prj.ScreenWidth * prj.ScreenWidth + prj.ScreenHeight * prj.ScreenHeight))
                    {
                        var color = frame.Color.Tint(nightMode);

                        var p = new PointF(prj.ScreenWidth / 2, prj.ScreenHeight / 2);
                        double rotAngle = 0;
                        if (cameraFrame.RotateOrigin == FovFrameRotateOrigin.Equatorial)
                        {
                            if (prj.ViewMode == ProjectionViewType.Horizontal)
                            {
                                rotAngle = prj.GetAxisRotation(prj.CenterEquatorial, -cameraFrame.Rotation);
                            }
                            else if (prj.ViewMode == ProjectionViewType.Equatorial)
                            {
                                rotAngle = -cameraFrame.Rotation;
                            }
                        }
                        else if (cameraFrame.RotateOrigin == FovFrameRotateOrigin.Horizontal)
                        {
                            if (prj.ViewMode == ProjectionViewType.Horizontal)
                            {
                                rotAngle = -cameraFrame.Rotation;
                            }
                            else if (prj.ViewMode == ProjectionViewType.Equatorial)
                            {
                                rotAngle = prj.GetAxisRotation(prj.CenterHorizontal, -cameraFrame.Rotation);
                            }
                        }

                        GL.PushMatrix();
                        GL.Translate(p.X, p.Y, 0);
                        GL.Rotate(rotAngle, 0, 0, 1);

                        GL.Enable(GL.BLEND);
                        GL.BlendFunc(GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA);
                        GL.Enable(GL.LINE_SMOOTH);
                        GL.LineWidth(1);

                        GL.Begin(GL.LINE_LOOP);
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
