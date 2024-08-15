using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Astrarium.Algorithms
{
    /// <summary>
    /// Represents a pair of the local horizontal coordinates.
    /// </summary>
    public class CrdsHorizontal
    {
        /// <summary>
        /// Globally defines Azimuth measurement origin.
        /// If true, azimuth is measured from North, oterwise from false.
        /// </summary>
        public static bool MeasureAzimuthFromNorth { get; set; }

        /// <summary>
        /// Azimuth, in degrees. Measured westwards from the south.
        /// </summary>
        public double Azimuth { get; set; }

        /// <summary>
        /// Altitude, in degrees. Positive above the horizon, negative below.
        /// </summary>
        public double Altitude { get; set; }

        /// <summary>
        /// Creates a pair of horizontal coordinates with empty values.
        /// </summary>
        public CrdsHorizontal() { }

        /// <summary>
        /// Creates a pair of horizontal coordinates with provided values of Azimuth and Altitude.
        /// </summary>
        /// <param name="azimuth">Azimuth, in degrees. Measured westwards from the south.</param>
        /// <param name="altitude">Altitude, in degrees. Positive above the horizon, negative below.</param>
        public CrdsHorizontal(double azimuth, double altitude)
        {
            Azimuth = Angle.To360(azimuth);
            Altitude = altitude;
        }

        /// <summary>
        ///  Creates a pair of horizontal coordinates by copying from other instance.
        /// </summary>
        /// <param name="other">Instance of coordinates to be copied.</param>
        public CrdsHorizontal(CrdsHorizontal other)
        {
            Azimuth = other.Azimuth;
            Altitude = other.Altitude;
        }

        /// <summary>
        /// Sets horizontal coordinates values.
        /// </summary>
        /// <param name="azimuth">Azimuth, in degrees. Measured westwards from the south.</param>
        /// <param name="altitude">Altitude, in degrees. Positive above the horizon, negative below.</param>
        public void Set(double azimuth, double altitude)
        {
            Azimuth = Angle.To360(azimuth);
            Altitude = altitude;
        }

        /// <summary>
        /// Sets horizontal coordinates values.
        /// </summary>
        /// <param name="other">Instance of coordinates to be copied.</param>
        public void Set(CrdsHorizontal other)
        {
            Azimuth = other.Azimuth;
            Altitude = other.Altitude;
        }

        public override string ToString()
        {
            return $"Az: {new DMS(Angle.To360(Azimuth + (MeasureAzimuthFromNorth ? 180 : 0)))}; Alt: {new DMS(Altitude)}";
        }

        public override bool Equals(object obj)
        {
            if (obj is CrdsHorizontal)
            {
                var h = obj as CrdsHorizontal;
                return h.Altitude == Altitude && h.Azimuth == h.Azimuth;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Altitude.GetHashCode();
                hash = hash * 23 + Azimuth.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Adds corrections to horizontal coordinates
        /// </summary>
        public static CrdsHorizontal operator +(CrdsHorizontal lhs, CrdsHorizontal rhs)
        {
            CrdsHorizontal h = new CrdsHorizontal();
            h.Azimuth = Angle.To360(lhs.Azimuth + rhs.Azimuth);
            h.Altitude = lhs.Altitude + rhs.Altitude;
            return h;
        }
    }
}
