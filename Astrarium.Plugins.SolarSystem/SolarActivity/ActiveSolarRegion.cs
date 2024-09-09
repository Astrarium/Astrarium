using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.SolarSystem
{
    public abstract class ActiveSolarRegion : SolarRegion
    {
        /// <summary>
        /// Region location, in heliographic degrees latitude and 
        /// degrees east or west from central meridian, rotated to 2400 UTC.
        /// </summary>
        public CrdsHeliographical Location { get; private set; } = new CrdsHeliographical();
    }
}
