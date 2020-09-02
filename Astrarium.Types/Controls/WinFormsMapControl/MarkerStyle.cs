using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    /// <summary>
    /// Defines visual style of the <see cref="Marker"/>.
    /// </summary>
    public class MarkerStyle
    {
        /// <summary>
        /// Pen to draw marker outline.
        /// </summary>
        public Pen MarkerPen { get; set; }

        /// <summary>
        /// Brush to fill marker interior.
        /// </summary>
        public Brush MarkerBrush { get; set; }

        /// <summary>
        /// Width of the marker circle, in pixels.
        /// </summary>
        public float MarkerWidth { get; set; }
        
        /// <summary>
        /// Brush to draw marker label.
        /// </summary>
        public Brush LabelBrush { get; set; }

        /// <summary>
        /// Font used to draw marker label.
        /// </summary>
        public Font LabelFont { get; set; }

        /// <summary>
        /// String format used to draw marker label.
        /// </summary>
        public StringFormat LabelFormat { get; set; }

        /// <summary>
        /// Creates new marker style.
        /// </summary>        
        public MarkerStyle() : this(Default.MarkerWidth, Default.MarkerBrush, Default.MarkerPen, Default.LabelBrush, Default.LabelFont, Default.LabelFormat) { }

        /// <summary>
        /// Creates new marker style.
        /// </summary>
        /// <param name="markerWidth">Width of the marker circle, in pixels.</param>
        public MarkerStyle(float markerWidth) : this(markerWidth, Default.MarkerBrush, Default.MarkerPen, Default.LabelBrush, Default.LabelFont, Default.LabelFormat) { }

        /// <summary>
        /// Creates new marker style.
        /// </summary>
        /// <param name="markerWidth">Width of the marker circle, in pixels.</param>
        /// /// <param name="markerBrush">Brush to fill marker interior.</param>
        public MarkerStyle(float markerWidth, Brush markerBrush) : this(markerWidth, markerBrush, Default.MarkerPen, Default.LabelBrush, Default.LabelFont, Default.LabelFormat) { }

        /// <summary>
        /// Creates new marker style.
        /// </summary>
        /// <param name="markerWidth">Width of the marker circle, in pixels.</param>
        /// <param name="markerBrush">Brush to fill marker interior.</param>
        /// <param name="markerPen">Pen to draw marker outline.</param>
        /// <param name="labelBrush">Brush to draw marker label.</param>
        /// <param name="labelFont">Font used to draw marker label.</param>
        /// <param name="labelFormat">String format used to draw marker label.</param>
        public MarkerStyle(float markerWidth, Brush markerBrush, Pen markerPen, Brush labelBrush, Font labelFont, StringFormat labelFormat)
        {
            MarkerPen = markerPen;
            LabelBrush = labelBrush;
            MarkerBrush = markerBrush;
            MarkerWidth = markerWidth;
            LabelFont = labelFont;
            LabelFormat = labelFormat;
        }

        /// <summary>
        /// Default marker style.
        /// </summary>
        public static MarkerStyle Default = new MarkerStyle(3, Brushes.Red, null, Brushes.Black, Drawing.SystemFonts.DefaultFont, StringFormat.GenericDefault);
    }
}
