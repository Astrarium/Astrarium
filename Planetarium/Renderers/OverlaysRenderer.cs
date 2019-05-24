using ADK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Renderers
{
    public class OverlaysRenderer : BaseRenderer
    {
        private Font fontLockMessage = new Font("Arial", 8);
        private readonly ISearcher searcher;

        public OverlaysRenderer(ISearcher searcher)
        {
            this.searcher = searcher;
        }

        public override void Render(IMapContext map)
        {
            DrawLockedText(map);
            DrawMeasureTool(map);
        }

        private void DrawLockedText(IMapContext map)
        {
            if (map.LockedObject != null && map.IsDragging)
            {
                var format = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                string text = $"Map locked on: {searcher.GetObjectName(map.LockedObject)}";

                PointF center = new PointF(map.Width / 2, map.Height / 2);
                var size = map.Graphics.MeasureString(text, fontLockMessage, center, format);
                int margin = 4;
                var box = new Rectangle((int)(center.X - size.Width / 2 - margin), (int)(center.Y - size.Height / 2 - margin), (int)size.Width + 2 * margin, (int)size.Height + 2 * margin);
                map.Graphics.FillRectangle(new SolidBrush(Color.Black), box);
                map.Graphics.DrawRectangle(new Pen(Color.FromArgb(100, Color.White)), box);
                map.Graphics.DrawString(text, fontLockMessage, new SolidBrush(Color.White), center, format);
            }
        }

        private void DrawMeasureTool(IMapContext map)
        {
            if (map.MeasureOrigin != null && map.MousePosition != null)
            {
                double coeff = map.DiagonalCoefficient();

                List<PointF> points = new List<PointF>();
                for (int f = 0; f <= 10; f++)
                {
                    CrdsHorizontal h = Angle.Intermediate(map.MousePosition, map.MeasureOrigin, f / 10.0);
                    points.Add(map.Project(h));
                    if (Angle.Separation(h, map.Center) > map.ViewAngle * coeff)
                    { 
                        break;
                    }
                }

                if (points.Count > 1)
                {
                    map.Graphics.DrawCurve(Pens.White, points.ToArray());
                    double angle = Angle.Separation(map.MousePosition, map.MeasureOrigin);
                    PointF p = map.Project(map.MousePosition);
                    map.Graphics.DrawString(Formatters.MeasuredAngle.Format(angle), fontLockMessage, Brushes.White, p.X + 5, p.Y + 5);
                }
            }
        }

        public override int ZOrder => 1000;
    }
}
