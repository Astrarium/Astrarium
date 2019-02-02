using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Objects
{
    /// <summary>
    /// Contains coordinates and visual appearance data for the Moon for given instant of time.
    /// </summary>
    public class Moon : SizeableCelestialObject
    { 
        /// <summary>
        /// Apparent topocentrical equatorial coordinates
        /// </summary>
        public CrdsEquatorial Equatorial { get; set; }

        /// <summary>
        /// Geocentrical ecliptical corrdinates
        /// </summary>
        public CrdsEcliptical Ecliptical0 { get; set; }

        /// <summary>
        /// Elongation angle, i.e. angular distance from the Sun. 
        /// Positive if eastern elongation, negative if western. 
        /// </summary>
        public double Elongation { get; set; }

        /// <summary>
        /// Phase of the Moon, from 0 (New Moon) to 1 (Full Moon).
        /// </summary>
        public double Phase { get; set; }

        /// <summary>
        /// Position angle of Moon axis, in degrees.
        /// </summary>
        public double PAaxis { get; set; }

        /// <summary>
        /// Libration elements for the Moon
        /// </summary>
        public Libration Libration { get; set; }

        /// <summary>
        /// Longitude of ascending node of lunar orbit
        /// </summary>
        public double AscendingNode { get; set; }
    }
}
