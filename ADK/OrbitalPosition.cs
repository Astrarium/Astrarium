using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK
{
    /// <summary>
    /// Pair of values: true anomaly "v" and radius-vector "r", representing position on the orbit
    /// </summary>
    internal class OrbitalPosition
    {
        /// <summary>
        /// True anomaly
        /// </summary>
        public double v { get; set; }

        /// <summary>
        /// Radius-vector
        /// </summary>
        public double r { get; set; }
    }
}
