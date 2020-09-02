using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    /// <summary>
    /// Defines visual style of the <see cref="Track"/>.
    /// </summary>
    public class TrackStyle
    {
        /// <summary>
        /// Pen used to draw track path.
        /// </summary>
        public Pen Pen { get; set; }

        /// <summary>
        /// Creates new <see cref="TrackStyle"/>.
        /// </summary>
        public TrackStyle()
        {

        }

        /// <summary>
        /// Creates new <see cref="TrackStyle"/>.
        /// </summary>
        /// <param name="pen"> Pen used to draw track path.</param>
        public TrackStyle(Pen pen) 
        {
            Pen = pen;
        }

        /// <summary>
        /// Default track style.
        /// </summary>
        public static TrackStyle Default = new TrackStyle(new Pen(Color.Blue) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash });
    }
}
