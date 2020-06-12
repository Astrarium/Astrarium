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
                if (frame is TelescopeFovFrame telescopeFovFrame)
                {
                    float radius = telescopeFovFrame.Size * 3600 / 2;
                    float size = map.GetDiskSize(radius);

                    if (frame.Shading > 0 && telescopeFovFrame.Size >= map.ViewAngle / 2)
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

        }
    }
}
