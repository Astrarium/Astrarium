namespace System.Windows.Forms
{
    /// <summary>
    /// Represents point on the Earth surface with geographical coordinates
    /// </summary>
    public struct GeoPoint
    {
        /// <summary>
        /// Longitude of the point, in degrees, from 0 to ±180, positive East, negative West. 0 is a point on prime meridian.
        /// </summary>
        public float Longitude { get; set; }

        /// <summary>
        /// Latitude of the point, in degrees, from +90 (North pole) to -90 (South Pole). 0 is a point on equator.
        /// </summary>
        public float Latitude { get; set; }

        /// <summary>
        /// Creates new instance of <see cref="GeoPoint"/> and initializes it with longitude and latitude values.
        /// </summary>
        /// <param name="longitude">Longitude of the point, in degrees, from 0 to ±180, positive East, negative West. 0 is a point on prime meridian.</param>
        /// <param name="latitude">Latitude of the point, in degrees, from +90 (North pole) to -90 (South Pole). 0 is a point on equator.</param>
        public GeoPoint(float longitude, float latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }
    }
}
