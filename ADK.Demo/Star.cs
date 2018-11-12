using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo
{
    public class Star : CelestialObject
    {
        /// <summary>
        /// Equatorial coordinates for the catalogue epoch
        /// </summary>
        public CrdsEquatorial Equatorial0 { get; set; } = new CrdsEquatorial();

        /// <summary>
        /// Apparent equatorial coordinates for the current epoch
        /// </summary>
        public CrdsEquatorial Equatorial { get; set; }

        /// <summary>
        /// Annual proper motion in RA J2000, FK5 system, arcsec/yr
        /// </summary>
        public float PmAlpha { get; set; }

        /// <summary>
        /// Annual proper motion in Dec J2000, FK5 system, arcsec/yr
        /// </summary>
        public float PmDelta { get; set; }

        /// <summary>
        /// Apparent magnitude of the star
        /// </summary>
        public float Mag { get; set; }

        /// <summary>
        /// Star color, i.e. spectral class
        /// </summary>
        public char Color { get; set; }
    }
}
