﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Astrarium.Algorithms
{
    /// <summary>
    /// Defines a point on the Earth surface.
    /// </summary>
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
        /// Utc offset, in hours
        /// </summary>
        public double UtcOffset { get; set; }

        /// <summary>
        /// Name of the location
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Alternative names of the location
        /// </summary>
        public string[] Names { get; set; }

        /// <summary>
        /// Country code (2 letters)
        /// </summary>
        public string Country { get; set; }

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

        public CrdsGeographical() { }

        public CrdsGeographical(double longitude, double latitude, double utcOffset = 0, double elevation = 0, string name = null)
        {
            Latitude = latitude;
            Longitude = longitude;
            UtcOffset = utcOffset;
            Elevation = elevation;
            Name = name;
        }

        public CrdsGeographical(DMS longitude, DMS latitude, double utcOffset = 0, double elevation = 0)
            : this(longitude.ToDecimalAngle(), latitude.ToDecimalAngle(), utcOffset, elevation) { }

        public CrdsGeographical(CrdsGeographical other)
            : this(other.Longitude, other.Latitude, other.UtcOffset, other.Elevation, other.Name) { }

        public override string ToString()
        {
            return $"Latitude: {new DMS(Latitude)}; Longitude:{new DMS(Longitude)}";
        }

        public override bool Equals(object obj)
        {
            if (GetHashCode() != obj.GetHashCode())
                return false;
            else if (obj is CrdsGeographical other)
                return
                    Name == other.Name &&
                    Elevation == other.Elevation &&
                    UtcOffset == other.UtcOffset &&
                    Latitude == other.Latitude &&
                    Longitude == other.Longitude;
            else 
                return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Name ?? "").GetHashCode();
                hash = hash * 23 + Elevation.GetHashCode();
                hash = hash * 23 + UtcOffset.GetHashCode();
                hash = hash * 23 + Latitude.GetHashCode();
                hash = hash * 23 + Longitude.GetHashCode();
                return hash;
            }
        }
    }
}
