using ADK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Objects
{
    public class JupiterMoon : SizeableCelestialObject
    {
        /// <summary>
        /// Apparent equatorial coordinates of the Galilean moon
        /// </summary>
        public CrdsEquatorial Equatorial { get; set; }

        /// <summary>
        /// Planetocentric rectangular coordinates of the Galilean moon
        /// </summary>
        public CrdsRectangular Planetocentric { get; set; }

        /// <summary>
        /// Planetocentric rectangular coordinates of the Galilean moon shadow
        /// </summary>
        public CrdsRectangular Shadow { get; set; }

        public CrdsEquatorial ShadowEquatorial { get; set; }

        public CrdsHorizontal ShadowHorizontal { get; set; }

        /// <summary>
        /// Visible shadow semidiameter, in seconds of arc
        /// </summary>
        public double ShadowSemidiameter { get; set; }

        /// <summary>
        /// Name of the Galilean moon
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Number of the Galilean moon (1 to 4)
        /// </summary>
        public int Number { get; set; }
    }
}
