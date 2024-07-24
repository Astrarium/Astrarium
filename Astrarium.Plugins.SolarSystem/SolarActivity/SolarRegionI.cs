using Astrarium.Algorithms;
using Astrarium.Plugins.SolarSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Astrarium.Plugins.SolarSystem
{
    /// <summary>
    /// Secribes a solar region with sunspots (section I)
    /// </summary>
    public class SolarRegionI: ActiveSolarRegion
    {
        /// <summary>
        /// Total corrected area of the group in millionths of the solar hemisphere.
        /// </summary>
        public int Area { get; set; }

        /// <summary>
        /// Modified Zurich classification of the group.
        /// </summary>
        public string Z { get; set; }

        /// <summary>
        /// Longitudinal extent of the group in heliographic degrees.
        /// </summary>
        public int LL { get; set; }

        /// <summary>
        /// Total number of visible sunspots in the group.
        /// </summary>
        public int NN { get; set; }

        /// <summary>
        /// Magnetic classification of the group.
        /// </summary>
        public string MagType { get; set; }
    }
}
