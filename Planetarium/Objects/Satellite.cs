using ADK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Objects
{
    public class Satellite : SizeableCelestialObject
    {
        /// <summary>
        /// Apparent equatorial coordinates of the satellite
        /// </summary>
        public CrdsEquatorial Equatorial { get; set; }

        /// <summary>
        /// Planetocentric rectangular coordinates of the satellite
        /// </summary>
        public CrdsRectangular Planetocentric { get; set; }

        /// <summary>
        /// Planetocentric rectangular coordinates of the satellite shadow
        /// </summary>
        public CrdsRectangular Shadow { get; set; }

        /// <summary>
        /// Name of the satellite
        /// </summary>
        public string Name { get; set; }
    }
}
