using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    /// <summary>
    /// Defines drawing style for <see cref="Polygon" /> object.
    /// </summary>
    public class PolygonStyle
    {
        /// <summary>
        /// Fill brush
        /// </summary>
        public Brush Brush { get; set; }

        /// <summary>
        /// Outline pen.
        /// </summary>
        public Pen Pen { get; set; }

        /// <summary>
        /// Creates new <see cref="PolygonStyle"/> with fill brush and without outline.
        /// </summary>
        /// <param name="brush">Fill brush.</param>
        public PolygonStyle(Brush brush)
        {
            Brush = brush;
        }

        /// <summary>
        /// Creates new <see cref="PolygonStyle"/> with outline and without fill.
        /// </summary>
        /// <param name="pen">Outline pen.</param>
        public PolygonStyle(Pen pen)
        {
            Pen = pen;
        }

        /// <summary>
        /// Creates new <see cref="PolygonStyle"/> with fill brush and outline pen.
        /// </summary>
        /// <param name="brush">Fill brush.</param>
        /// <param name="pen">Outline pen.</param>
        public PolygonStyle(Brush brush, Pen pen) 
        {
            Brush = brush;
            Pen = pen;
        }


        /// <summary>
        /// Default polygon style.
        /// </summary>
        public static PolygonStyle Default = new PolygonStyle(new SolidBrush(Color.FromArgb(100, Color.Black)), new Pen(Color.Black) { Width = 2 });
    }
}
