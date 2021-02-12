namespace Astrarium.Algorithms
{
    /// <summary>
    /// Contains details of local circumstances of the lunar eclipse for a given place.
    /// </summary>
    public class LunarEclipseLocalCircumstances
    {
        /// <summary>
        /// Geographical location.
        /// </summary>
        public CrdsGeographical Location { get; set; }

        /// <summary>
        /// Details for instant of beginning of penumbral phase of lunar eclipse.
        /// </summary>
        public LunarEclipseLocalCircumstancesContactPoint PenumbralBegin { get; set; }

        /// <summary>
        /// Details for instant of beginning of partial phase of lunar eclipse.
        /// </summary>
        public LunarEclipseLocalCircumstancesContactPoint PartialBegin { get; set; }

        /// <summary>
        /// Details for instant of beginning of total phase of lunar eclipse.
        /// </summary>
        public LunarEclipseLocalCircumstancesContactPoint TotalBegin { get; set; }

        /// <summary>
        /// Details for instant of maximum of lunar eclipse.
        /// </summary>
        public LunarEclipseLocalCircumstancesContactPoint Maximum { get; set; }

        /// <summary>
        /// Details for instant of end of total phase of lunar eclipse.
        /// </summary>
        public LunarEclipseLocalCircumstancesContactPoint TotalEnd { get; set; }

        /// <summary>
        /// Details for instant of end of partial phase of lunar eclipse.
        /// </summary>
        public LunarEclipseLocalCircumstancesContactPoint PartialEnd { get; set; }

        /// <summary>
        /// Details for instant of end of penumbral phase of lunar eclipse.
        /// </summary>
        public LunarEclipseLocalCircumstancesContactPoint PenumbralEnd { get; set; }
    }

    /// <summary>
    /// Describes details of instant of lunar eclipse for a given point.
    /// </summary>
    public class LunarEclipseLocalCircumstancesContactPoint
    {
        /// <summary>
        /// Julian day of instant.
        /// </summary>
        public double JulianDay { get; private set; }

        /// <summary>
        /// Local altitude of the Moon at the instant.
        /// </summary>
        public double LunarAltitude { get; private set; }

        /// <summary>
        /// Creates new instance
        /// </summary>
        public LunarEclipseLocalCircumstancesContactPoint(double jd, double alt)
        {
            JulianDay = jd;
            LunarAltitude = alt;
        }
    }
}
