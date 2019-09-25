using ADK;
using Planetarium.Types;
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
        private Font fontDiagnosticText = new Font("Monospace", 8);
        private readonly ISearcher searcher;

        public OverlaysRenderer(ISearcher searcher)
        {
            this.searcher = searcher;
        }

        public override void Render(IMapContext map)
        {
            DrawLockedText(map);
            DrawMeasureTool(map);
            DrawDiagnostic(map);
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
            
        }

        private void DrawDiagnostic(IMapContext map)
        {
            map.Graphics.DrawString($"FOV: {Formatters.MeasuredAngle.Format(map.ViewAngle)}\nMag limit: {Formatters.Magnitude.Format(map.MagLimit)}\nFPS: {map.FPS}", fontDiagnosticText, Brushes.Red, new PointF(10, 10));
        }

        public override RendererOrder Order => RendererOrder.Foreground;

        public override string Name => "Overlays";
    }
}
