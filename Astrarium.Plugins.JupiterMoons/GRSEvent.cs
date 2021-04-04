namespace Astrarium.Plugins.JupiterMoons
{
    /// <summary>
    /// Represents circumstances of Great Red Spot visibility.
    /// </summary>
    public class GRSEvent
    {
        /// <summary>
        /// Instant of GRS transit.
        /// </summary>
        public double JdTransit { get; set; }
        
        /// <summary>
        /// GRS appearing time. It's always about 2 hours earlier than GRS transit time instant.
        /// </summary>
        public double JdAppear { get; set; }

        /// <summary>
        /// GRS appearing time. It's always about 2 hours later than GRS transit time instant.
        /// </summary>
        public double JdDisappear { get; set; }

        /// <summary>
        /// Altitude of the Sun above the horison at the instant of GRS transit, in degrees.
        /// </summary>
        public double SunAltTransit { get; set; }

        /// <summary>
        /// Altitude of the Jupiter above the horison at the instant of GRS transit, in degrees.
        /// </summary>
        public double JupiterAltTransit { get; set; }

        /// <summary>
        /// Altitude of the Sun above the horison at the instant of GRS appearing, in degrees.
        /// </summary>
        public double SunAltAppear { get; set; }

        /// <summary>
        /// Altitude of the Jupiter above the horison at the instant of GRS appearing, in degrees.
        /// </summary>
        public double JupiterAltAppear { get; set; }

        /// <summary>
        /// Altitude of the Sun above the horison at the instant of GRS disappearing, in degrees.
        /// </summary>
        public double SunAltDisappear { get; set; }

        /// <summary>
        /// Altitude of the Jupiter above the horison at the instant of GRS disappearing, in degrees.
        /// </summary>
        public double JupiterAltDisappear { get; set; }
    }
}
