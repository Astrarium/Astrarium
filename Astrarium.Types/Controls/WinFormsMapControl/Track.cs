using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    /// <summary>
    /// Represents track (collection of connected points).
    /// </summary>
    public class Track : List<GeoPoint>
    {
        /// <summary>
        /// Style to draw the track
        /// </summary>
        public TrackStyle Style { get; set; }

        /// <summary>
        /// Custom data associated with the marker
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Creates a track with specified style
        /// </summary>
        /// <param name="style"></param>
        public Track(TrackStyle style)
        {
            Style = style;
        }
    }
}
