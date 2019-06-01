using System;
using System.Collections.Generic;
using System.Text;

namespace ADK
{
    public class CrdsGeographical
    {
        /// <summary>
        /// Latitude, in degrees. 
        /// Measured from +90 (North pole) to -90 (South pole).
        /// </summary> 
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude, in degrees. Positive west, negative east to Greenwich.
        /// Measured from -180 to +180.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Elevation above the sea level, in meters
        /// </summary>
        public double Elevation { get; set; }

        /// <summary>
        /// Term needed for calculation of parallax effect.
        /// </summary>
        /// <remarks>
        /// Taken from from PAWC, p.66.
        /// </remarks>
        public double RhoCosPhi
        {
            get
            {
                double latitude = Angle.ToRadians(Latitude);
                double u = Math.Atan(0.99664719 * Math.Tan(latitude));
                return Math.Cos(u) + Elevation / 6378140.0 * Math.Cos(latitude);               
            }
        }

        /// <summary>
        /// Term needed for calculation of parallax effect.
        /// </summary>
        /// <remarks>
        /// Taken from from PAWC, p.66.
        /// </remarks>
        public double RhoSinPhi
        {
            get
            {
                double latitude = Angle.ToRadians(Latitude);
                double u = Math.Atan(0.99664719 * Math.Tan(latitude));
                return 0.99664719 * Math.Sin(u) + Elevation / 6378140.0 * Math.Sin(latitude);
            }
        }

        /// <summary>
        /// Utc offset, in hours
        /// </summary>
        public double UtcOffset { get; set; }

        /// <summary>
        /// Time Zone Id
        /// </summary>
        public string TimeZoneId { get; set; }

        /// <summary>
        /// Name of the location
        /// </summary>
        public string LocationName { get; set; }

        public CrdsGeographical() { }

        public CrdsGeographical(double longitude, double latitude, double utcOffset = 0, double elevation = 0, string timeZoneId = null, string locationName = null)
        {
            Latitude = latitude;
            Longitude = longitude;
            UtcOffset = utcOffset;
            Elevation = elevation;
            TimeZoneId = timeZoneId;
            LocationName = locationName;
        }

        public CrdsGeographical(DMS longitude, DMS latitude, double utcOffset = 0, double elevation = 0)
            : this(longitude.ToDecimalAngle(), latitude.ToDecimalAngle(), utcOffset, elevation) { }

        public CrdsGeographical(CrdsGeographical other)
            : this(other.Longitude, other.Latitude, other.UtcOffset, other.Elevation, other.TimeZoneId, other.LocationName) { }

        public override string ToString()
        {
            return $"Latitude: {new DMS(Latitude)}; Longitude:{new DMS(Longitude)}";
        }
    }
}
