namespace System.Windows.Forms
{
    /// <summary>
    /// Represents marker point on the map.
    /// </summary>
    public class Marker
    {
        /// <summary>
        /// Marker coordinates.
        /// </summary>
        public GeoPoint Point { get; set; }

        /// <summary>
        /// Style to draw the marker.
        /// </summary>
        public MarkerStyle Style { get; set; }

        /// <summary>
        /// Marker label.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Custom data associated with the marker.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Creates new <see cref="Marker"/> object with specified coordinates.
        /// </summary>
        /// <param name="point">Coordinates of the marker.</param>
        public Marker(GeoPoint point)
        {
            Point = point;
        }

        /// <summary>
        /// Creates new <see cref="Marker"/> object with specified coordinates and style.
        /// </summary>
        /// <param name="point">Coordinates of the marker.</param>
        /// <param name="style">Marker style.</param>
        public Marker(GeoPoint point, MarkerStyle style) 
        {
            Point = point;
            Style = style;            
        }

        /// <summary>
        /// Creates new <see cref="Marker"/> object with specified coordinates and label.
        /// </summary>
        /// <param name="point">Coordinates of the marker.</param>
        /// <param name="label">Marker label.</param>
        public Marker(GeoPoint point, string label)
        {
            Point = point;
            Label = label;
        }

        /// <summary>
        /// Creates new <see cref="Marker"/> object with specified coordinates, style and label.
        /// </summary>
        /// <param name="point">Coordinates of the marker.</param>
        /// <param name="style">Marker style.</param>
        /// <param name="label">Marker label.</param>
        public Marker(GeoPoint point, MarkerStyle style, string label)
        {
            Point = point;
            Style = style;
            Label = label;
        }
    }
}
