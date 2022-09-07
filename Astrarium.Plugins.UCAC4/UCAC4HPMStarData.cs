using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.UCAC4
{
    /// <summary>
    /// Contains data for High Proper Motions stars of the UCAC4 catalog
    /// </summary>
    internal class UCAC4HPMStarData
    {
        /// <summary>
        /// Zone number, 1...900
        /// </summary>
        public int ZoneNumber { get; set; }

        /// <summary>
        /// Running number of the star in the zone, 1-based
        /// </summary>
        public int RunningNumber { get; set; }

        /// <summary>
        /// Proper motion in RA*Cos(Dec)
        /// </summary>
        public int PmRac { get; set; }

        /// <summary>
        /// Proper motion in Dec
        /// </summary>
        public int PmDc { get; set; }
    }
}
