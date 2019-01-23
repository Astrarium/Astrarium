using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Objects
{
    public class Sun : SizeableCelestialObject
    {
        /// <summary>
        /// Apparent topocentrical equatorial coordinates
        /// </summary>
        public CrdsEquatorial Equatorial { get; set; }

        public CrdsEcliptical Ecliptical { get; set; }
    }
}
