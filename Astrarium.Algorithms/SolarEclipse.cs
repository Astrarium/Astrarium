using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Algorithms
{
    /// <summary>
    /// Describes general details of the Solar eclipse
    /// </summary>
    public class SolarEclipse
    {
        /// <summary>
        /// Meeus lunation number
        /// </summary>
        public int MeeusLunationNumber { get; set; }

        /// <summary>
        /// Instant of maximal eclipse
        /// </summary>
        public double JulianDayMaximum { get; set; }

        /// <summary>
        /// Eclipse magnitude
        /// </summary>
        public double Magnitude { get; set; } = 1;

        /// <summary>
        /// Least distance from the axis of the Moon's shadow to the center of the Earth,
        /// in units of equatorial radius of the Earth.
        /// </summary>
        public double Gamma { get; set; }

        /// <summary>
        /// Radius of the Moon's umbral cone in the fundamental plane,
        /// in units of equatorial radius of the Earth.
        /// </summary>
        public double U { get; set; }

        /// <summary>
        /// Type of eclipse: annular, central, hybrid (annular-central) or partial
        /// </summary>
        public SolarEclipseType EclipseType { get; set; }

        /// <summary>
        /// Flag indicating the eclipse is non-central
        /// (umbral cone touches the Earth polar regio but umbral axis does not)
        /// </summary>
        public bool IsNonCentral { get; set; }

        /// <summary>
        /// Flag indicating the eclipse occurance is uncertain 
        /// and needs to be verified with mo accurate algorithm.
        /// </summary>
        public bool IsUncertain { get; set; }

        /// <summary>
        /// Saros series number for the eclipse
        /// </summary>
        public int Saros { get; set; }
    }
}
