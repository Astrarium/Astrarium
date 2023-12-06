using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Types
{
    /// <summary>
    /// Set of extensions for <see cref="Graphics"/> class.
    /// </summary>
    public static class GraphicsExtensions
    {
        private static ImageAttributes RedImageAttributes = null;
        private static ImageAttributes WhiteImageAttributes = null;

        private static ImageAttributes GetImageAttributes(bool nightMode)
        {
            if (nightMode)
            {
                if (RedImageAttributes == null)
                {
                    float[][] matrix = {
                        new float[] {0.3f, 0, 0, 0, 0},
                        new float[] {0.3f, 0, 0, 0, 0},
                        new float[] {0.3f, 0, 0, 0, 0},
                        new float[] {0, 0, 0, 1, 0},
                        new float[] {0, 0, 0, 0, 0}
                    };
                    var colorMatrix = new ColorMatrix(matrix);
                    RedImageAttributes = new ImageAttributes();
                    RedImageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                }
                return RedImageAttributes;
            }
            else
            {
                return null;
            }
        }

        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, RectangleF bounds, float cornerRadius)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            if (pen == null)
                throw new ArgumentNullException("pen");

            using (GraphicsPath path = RoundedRect(bounds, cornerRadius))
            {
                graphics.DrawPath(pen, path);
            }
        }

        private static GraphicsPath RoundedRect(RectangleF bounds, float radius)
        {
            float diameter = radius * 2;
            SizeF size = new SizeF(diameter, diameter);
            RectangleF arc = new RectangleF(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        public static Color Tint(this Color color, bool nightMode)
        {
            if (nightMode)
            {
                byte r = new byte[] { color.R, color.G, color.B }.Max();
                return Color.FromArgb(color.A, r, 0, 0);
            }
            else
            {
                return color;
            }
        }
    }
}
