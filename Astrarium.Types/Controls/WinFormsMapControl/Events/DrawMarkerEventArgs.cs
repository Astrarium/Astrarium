using System.Drawing;

namespace System.Windows.Forms
{
    /// <summary>
    /// Provides data for <see cref="MapControl.DrawMarker"/> event.
    /// </summary>
    public class DrawMarkerEventArgs : MapControlDrawEventArgs
    {
        /// <summary>
        /// <see cref="System.Windows.Forms.Marker"/> instance to be drawn.
        /// </summary>
        public Marker Marker { get; internal set; }

        /// <summary>
        /// Coordinates of the marker on the map.
        /// </summary>
        public PointF Point { get; internal set; }

        /// <summary>
        /// Creates new instance of <see cref="DrawMarkerEventArgs"/>.
        /// </summary>
        internal DrawMarkerEventArgs() { }
    }
}
