using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.SolarSystem
{
    public abstract class SolarRegion
    {
        /// <summary>
        /// SESC region number.
        /// </summary>
        public int Nmbr { get; set; }

        /// <summary>
        /// Carrington longitude of the region.
        /// </summary>
        public int Lo { get; set; }
    }
}
