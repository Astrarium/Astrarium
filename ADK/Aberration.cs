using System;
using System.Collections.Generic;
using System.Text;

namespace ADK
{
    /// <summary>
    /// Contains methods for calculation of aberration effects.
    /// </summary>
    public static class Aberration
    {
        /// <summary>
        /// Calculates the aberration effect for the Sun.
        /// </summary>
        /// <param name="distance">Distance Sun-Earth, in astronomical units.</param>
        public static CrdsEcliptical AberrationEffect(double distance)
        {
            return new CrdsEcliptical(-20.4898 / distance / 3600, 0);
        }        
    }
}
