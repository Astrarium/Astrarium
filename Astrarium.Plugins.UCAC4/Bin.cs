using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.UCAC4
{
    /// <summary>
    /// Represents part of UCAC4 catalog zone, i.e. area of the sky with dimensions 0.2 * 0.25 sq. degrees
    /// </summary>
    internal struct Bin
    {
        /// <summary>
        /// Running star number (index along the main data file)
        /// of the star before the first one in this bin,
        /// the sequence starts out with 0 at the beginning of
        /// each new declination zone
        /// </summary>
        public uint N0;

        /// <summary>
        /// Number of stars in this bin (which can be zero)
        /// </summary>
        public uint NN;

        /// <summary>
        /// Zone number (1 to 900), starting from South Celestial Pole
        /// </summary>
        public int ZN;

        /// <summary>
        /// Index for bins along RA(1 to 1440)
        /// </summary>
        public int J;
    }
}
