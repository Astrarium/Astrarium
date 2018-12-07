using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Renderers
{
    /// <summary>
    /// Set of extensions for <see cref="Graphics"/> class.
    /// </summary>
    public static class GraphicsExtenstions
    {
        public static void DrawXCross(this Graphics g, Pen pen, PointF p, float size)
        {
            g.DrawLine(pen, p.X - size, p.Y - size, p.X + size, p.Y + size);
            g.DrawLine(pen, p.X + size, p.Y - size, p.X - size, p.Y + size);
        }

        public static void DrawPlusCross(this Graphics g, Pen pen, PointF p, float size)
        {
            g.DrawLine(pen, p.X, p.Y - size, p.X, p.Y + size);
            g.DrawLine(pen, p.X + size, p.Y, p.X - size, p.Y);
        }

        public static void DrawStringOpaque(this Graphics g, string s, Font font, Brush textBrush, Brush bgBrush, PointF p, StringFormat format)
        {
            var size = g.MeasureString(s, font);
            PointF pBox = new PointF(p.X, p.Y);
            if (format.Alignment == StringAlignment.Center)
            {
                pBox.X -= size.Width / 2;
            }
            if (format.LineAlignment == StringAlignment.Center)
            {
                pBox.Y -= size.Height / 2;
            }
            g.FillRectangle(bgBrush, new RectangleF(pBox, size));
            g.DrawString(s, font, textBrush, p, format);
        }

        public static void DrawStringOpaque(this Graphics g, string s, Font font, Brush textBrush, Brush bgBrush, PointF p)
        {
            g.DrawStringOpaque(s, font, textBrush, bgBrush, p, StringFormat.GenericTypographic);
        }
    }
}
