using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.JupiterMoons
{
    public class GRSEvent
    {
        /// <summary>
        /// Instant of GRS transit.
        /// </summary>
        public double JdTransit { get; set; }
        
        /// <summary>
        /// GRS appearing time. It's always about 2 hours earlier than GRS transit time instant.
        /// </summary>
        public double JdAppear { get; set; }

        /// <summary>
        /// GRS appearing time. It's always about 2 hours later than GRS transit time instant.
        /// </summary>
        public double JdDisappear { get; set; }

        public double SunAltTransit { get; set; }
        public double JupiterAltTransit { get; set; }

        public double SunAltAppear { get; set; }
        public double JupiterAltAppear { get; set; }

        public double SunAltDisappear { get; set; }
        public double JupiterAltDisappear { get; set; }
    }
}
