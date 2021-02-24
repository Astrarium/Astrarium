using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Algorithms
{
    /// <summary>
    /// Defines types of lunar eclipses
    /// </summary>
    public enum LunarEclipseType
    {
        /// <summary>
        /// Total umbral eclipse
        /// </summary>
        Total = 0,

        /// <summary>
        /// Partial umbral eclipse
        /// </summary>
        Partial = 1,

        /// <summary>
        /// Penumbral eclpse
        /// </summary>
        Penumbral = 2
    }
}
