using System;
using System.Collections.Generic;
using System.Text;

namespace ADK
{
    public class CrdsGeographical
    {
        /// <summary>
        /// Latitude, in degrees.
        /// </summary> 
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude, in degrees.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Elevation above the sea level, in meters
        /// </summary>
        public double Elevation { get; set; }

        public CrdsGeographical() { }

        public CrdsGeographical(double latitude, double longitude, double elevation = 0)
        {
            Latitude = latitude;
            Longitude = longitude;
            Elevation = elevation;
        }

        public CrdsGeographical(DMS latitude, DMS longitude, double elevation = 0)
        {
            Latitude = latitude.ToDecimalAngle();
            Longitude = longitude.ToDecimalAngle();
            Elevation = elevation;
        }
    }
}
