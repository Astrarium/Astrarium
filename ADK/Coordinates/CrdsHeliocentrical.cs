using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ADK
{
    /// <summary>
    /// Heiocentrical VSOP87 coordinates
    /// </summary>
    public class CrdsHeliocentrical
    {
        /// <summary>
        /// Longitude, in degrees
        /// </summary>
        public double L { get; set; }

        /// <summary>
        /// Latitude, in degrees
        /// </summary>
        public double B { get; set; }

        /// <summary>
        /// Radius-vector, in astronomical units
        /// </summary>
        public double R { get; set; }

        public static CrdsHeliocentrical operator+(CrdsHeliocentrical lhs, CrdsHeliocentrical rhs)
        {
            return new CrdsHeliocentrical()
            {
                L = Angle.To360(lhs.L + rhs.L),
                B = lhs.B + rhs.B,
                R = lhs.R + rhs.R,
            };
        }
    }
}
