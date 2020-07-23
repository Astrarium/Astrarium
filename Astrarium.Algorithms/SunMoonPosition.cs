namespace Astrarium.Algorithms
{
    /// <summary>
    /// Represents Sun and Moon positions data
    /// required for calculating solar eclipses
    /// </summary>
    public class SunMoonPosition
    {
        /// <summary>
        /// Julian day of position data
        /// </summary>
        public double JulianDay { get; set; }

        /// <summary>
        /// Geocentrical equatorial coordinates of the Sun at the moment
        /// </summary>
        public CrdsEquatorial Sun { get; set; }

        /// <summary>
        /// Geocentrical equatorial coordinates of the Moon at the moment
        /// </summary>
        public CrdsEquatorial Moon { get; set; }

        /// <summary>
        /// Distance Earth-Sun center, in units of Earth equatorial radii.
        /// </summary>
        public double DistanceSun { get; set; }

        /// <summary>
        /// Distance Earth-Moon center, in units of Earth equatorial radii.
        /// </summary>
        public double DistanceMoon { get; set; }
    }
}
