using System;
using System.Collections.Generic;
using System.Text;

namespace ADK
{
    /// <summary>
    /// Represents a pair of equatorial coordinates.
    /// </summary>
    public class CrdsEquatorial
    {
        /// <summary>
        /// Right Ascension, in degrees, from 0 to 360.
        /// </summary>
        public double Alpha { get; set; }
        /// <summary>
        /// Declination, in degrees, from -90 to +90. Positive if north of the celestial equator, negative if south.
        /// </summary>
        public double Delta { get; set; }

        public CrdsEquatorial() { }

        public CrdsEquatorial(double alpha, double delta)
        {
            Alpha = alpha;
            Delta = delta;
        }
    }
}
