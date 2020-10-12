using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.FOV
{
    public class FieldOfViewRenderer : BaseRenderer
    {
        private ISettings settings;

        private Font font = new Font("Arial", 8);
        private StringFormat format = new StringFormat() { Alignment = StringAlignment.Center };

        public override RendererOrder Order => RendererOrder.Foreground;

        public FieldOfViewRenderer(ISettings settings)
        {
            this.settings = settings;
        }

        public override void Render(IMapContext map)
        {
            var frames = settings.Get<List<FovFrame>>("FovFrames").Where(f => f.Enabled);

            foreach (var frame in frames)
            {
                if (frame is CircularFovFrame circularFrame)
                {
                    float radius = circularFrame.Size * 3600 / 2;
                    float size = map.GetDiskSize(radius);

                    // do not draw frame if its size exceeds screen bounds
                    if (size < Math.Sqrt(map.Width * map.Width + map.Height * map.Height))
                    {
                        if (frame.Shading > 0 && circularFrame.Size >= map.ViewAngle / 2)
                        {
                            var circle = new GraphicsPath();
                            circle.AddEllipse(map.Width / 2 - size / 2, map.Height / 2 - size / 2, size, size);

                            var shading = new Region(new RectangleF(0, 0, map.Width, map.Height));
                            shading.Exclude(circle);

                            int transparency = (int)(frame.Shading / 100f * 255);
                            var solidBrush = new SolidBrush(Color.FromArgb(transparency, map.GetSkyColor()));
                            map.Graphics.FillRegion(solidBrush, shading);
                        }
                        map.Graphics.DrawEllipse(new Pen(frame.Color), map.Width / 2 - size / 2, map.Height / 2 - size / 2, size, size);

                        float labelWidth = map.Graphics.MeasureString(frame.Label, font).Width;
                        if (labelWidth <= size * 2)
                        {
                            map.Graphics.DrawString(frame.Label, font, new SolidBrush(frame.Color), new PointF(map.Width / 2, map.Height / 2 + size / 2), format);
                        }
                    }
                }
                else if (frame is CameraFovFrame cameraFrame)
                {
                    float width = map.GetDiskSize(cameraFrame.Width * 3600);
                    float height = map.GetDiskSize(cameraFrame.Height * 3600);

                    // do not draw frame if its size exceeds screen bounds
                    if (Math.Min(width, height) < Math.Sqrt(map.Width * map.Width + map.Height * map.Height))
                    {
                        var eqCenter = map.Center.ToEquatorial(map.GeoLocation, map.SiderealTime);
                        float angle = map.GetRotationTowardsNorth(eqCenter) + cameraFrame.Rotation;

                        map.Graphics.TranslateTransform(map.Width / 2, map.Height / 2);
                        map.Graphics.RotateTransform(angle);

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

                        map.Graphics.DrawRectangle(new Pen(frame.Color), -width / 2, -height / 2, width, height);
                        float labelWidth = map.Graphics.MeasureString(frame.Label, font).Width;
                        if (labelWidth <= width * 2)
                        {
                            map.Graphics.DrawString(frame.Label, font, new SolidBrush(frame.Color), new PointF(0, height / 2), format);
                        }

                        map.Graphics.ResetTransform();
                    }
                }
            }

        }
    }
}
