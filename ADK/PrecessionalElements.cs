using System;
using System.Collections.Generic;
using System.Text;

namespace ADK
{
    /// <summary>
    /// Describes triplet of precessional elements 
    /// needed for reduction of coordinates from one epoch to another.
    /// </summary>
    public class PrecessionalElements
    {
        /// <summary>
        /// ζ
        /// </summary>
        public double zeta { get; set; }

        /// <summary>
        /// z
        /// </summary>
        public double z { get; set; }

        /// <summary>
        /// θ
        /// </summary>
        public double theta { get; set; }

        /// <summary>
        /// Initial epoch, in Julian Days.
        /// </summary>
        public double InitialEpoch { get; set; }

        /// <summary>
        /// Target epoch, in Julian Days.
        /// </summary>
        public double TargetEpoch { get; set; }
    }
}
