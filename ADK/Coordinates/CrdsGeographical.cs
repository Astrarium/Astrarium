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

        public double RhoCosPhi { get; private set; }

        public double RhoSinPhi { get; private set; }

        /// <summary>
        /// Utc offset, in hours
        /// </summary>
        public double UtcOffset { get; set; }

        public CrdsGeographical(double latitude, double longitude, double utcOffset = 0, double elevation = 0)
        {
            Latitude = latitude;
            Longitude = longitude;
            UtcOffset = utcOffset;
            Elevation = elevation;
            CalculateParallaxTerms();
        }

        public CrdsGeographical(DMS latitude, DMS longitude, double utcOffset = 0, double elevation = 0) 
            : this(latitude.ToDecimalAngle(), longitude.ToDecimalAngle(), utcOffset, elevation) { }

        /// <summary>
        /// Calculates terms needed for calculation of parallax effect.
        /// </summary>
        /// <remarks>
        /// This method is taken from PAWC, p.66.
        /// </remarks>
        private void CalculateParallaxTerms()
        {
            double latitude = Angle.ToRadians(Latitude);
            double u = Math.Atan(0.99664719 * Math.Tan(latitude));
            RhoCosPhi = Math.Cos(u) + Elevation / 6378140.0 * Math.Cos(latitude);
            RhoSinPhi = 0.99664719 * Math.Sin(u) + Elevation / 6378140.0 * Math.Sin(latitude);
        }
    }
}
