using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Objects
{
    public class CelestialPoint
    {
        /// <summary>
        /// Equatorial coordinates of a point referred to initial epoch
        /// </summary>
        public CrdsEquatorial Equatorial0 { get; set; } = new CrdsEquatorial();

        /// <summary>
        /// Local horizontal coordinates
        /// </summary>
        public CrdsHorizontal Horizontal { get; set; }
    }
}
