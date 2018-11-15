using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Objects
{
    public class ConstBorderPoint
    {
        public bool Start { get; set; }

        /// <summary>
        /// Equatorial coordinates of a point referred to J2000.0 epoch
        /// </summary>
        public CrdsEquatorial Equatorial0 { get; set; } = new CrdsEquatorial();

        /// <summary>
        /// Equatorial coordinates of a point referred to target epoch
        /// </summary>
        public CrdsEquatorial Equatorial { get; set; }

        /// <summary>
        /// Local horizontal coordinates
        /// </summary>
        public CrdsHorizontal Horizontal { get; set; }
    }
}
