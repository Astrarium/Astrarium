using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Algorithms
{
    public class InstantLunarEclipseElements
    {
        /// <summary>
        /// Instant of the elements.
        /// </summary>
        public double JulianDay { get; set; }

        /// <summary>
        /// DeltaT value (difference between Dynamical and Universal Times).
        /// If not specified, calculated automatically for the <see cref="JulianDay"/> value.
        /// </summary>
        public double DeltaT { get; set; }

        /// <summary>
        /// X-coordinate of center of the Moon in fundamental plane.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y-coordinate of center of the Moon in fundamental plane.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Earth penumbra radius, in degrees.
        /// </summary>
        public double F1 { get; set; }

        /// <summary>
        /// Earth umbra radius, in degrees.
        /// </summary>
        public double F2 { get; set; }

        /// <summary>
        /// Lunar radius (semidiameter), in degrees.
        /// </summary>
        public double F3 { get; set; }

        /// <summary>
        /// Geocentric right ascension of the Moon, in degrees.
        /// </summary>
        public double Alpha { get; set; }

        /// <summary>
        /// Geocentric declination of the Moon, in degrees.
        /// </summary>
        public double Delta { get; set; }
    }
}
