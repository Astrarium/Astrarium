using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Astrarium.Plugins.SolarSystem
{
    /// <summary>
    /// Descibes an active region that where observed on the previous solar 
    /// rotation and is due to reappear on the East limb in the 
    /// next 3 days.
    /// </summary>
    public class SolarRegionII
    {
        /// <summary>
        /// SESC region number.
        /// </summary>
        public int Nmbr { get; set; }

        /// <summary>
        /// Heliographic degrees latitude of the group on its last disk passage.
        /// </summary>
        public int Lat { get; set; }

        /// <summary>
        /// Carrington longitude of the group on its last disk passage.
        /// </summary>
        public int Lo { get; set; }
    }
}
