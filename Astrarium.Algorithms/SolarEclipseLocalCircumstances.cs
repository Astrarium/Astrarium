namespace Astrarium.Algorithms
{
    /// <summary>
    /// Contains details of local circumstances of the solar eclipse for a given place.
    /// </summary>
    public class SolarEclipseLocalCircumstances
    {
        /// <summary>
        /// Flag indicating the eclipse is invisible from current point.
        /// </summary>
        public bool IsInvisible { get; set; }

        /// <summary>
        /// Geographical location.
        /// </summary>
        public CrdsGeographical Location { get; set; }

        /// <summary>
        /// Details for instant of beginning of partial phase of solar eclipse.
        /// </summary>
        public SolarEclipseLocalCircumstancesContactPoint PartialBegin { get; set; }

        /// <summary>
        /// Details for instant of beginning of total phase of solar eclipse.
        /// </summary>
        public SolarEclipseLocalCircumstancesContactPoint TotalBegin { get; set; }

        /// <summary>
        /// Details for instant of maximum of solar eclipse.
        /// </summary>
        public SolarEclipseLocalCircumstancesContactPoint Maximum { get; set; }

        /// <summary>
        /// Details for instant of end of total phase of solar eclipse.
        /// </summary>
        public SolarEclipseLocalCircumstancesContactPoint TotalEnd { get; set; }

        /// <summary>
        /// Details for instant of end of partial phase of solar eclipse.
        /// </summary>
        public SolarEclipseLocalCircumstancesContactPoint PartialEnd { get; set; }

        /// <summary>
        /// Maximal eclipse magnitude for a given place.
        /// </summary>
        public double MaxMagnitude { get; set; }

        /// <summary>
        /// Moon/Sun visible diameters ratio.
        /// </summary>
        public double MoonToSunDiameterRatio { get; set; }

        /// <summary>
        /// Total path width, in kilometers, for total or annular phases only.
        /// </summary>
        public double PathWidth { get; set; }

        /// <summary>
        /// Duration of total phase, in fractions of day.
        /// </summary>
        public double TotalDuration => TotalBegin != null && TotalEnd != null ? TotalEnd.JulianDay - TotalBegin.JulianDay : 0;

        /// <summary>
        /// Duration of partial phase, in fractions of day.
        /// </summary>
        public double PartialDuration => PartialBegin != null && PartialEnd != null ? PartialEnd.JulianDay - PartialBegin.JulianDay : 0;
    }

    /// <summary>
    /// Describes details of instant of solar eclipse for a given point.
    /// </summary>
    public class SolarEclipseLocalCircumstancesContactPoint
    {
        /// <summary>
        /// Julian Day of the contact instant
        /// </summary>
        public double JulianDay { get; private set; }

        /// <summary>
        /// Solar altitude, in degrees, at the instant
        /// </summary>
        public double SolarAltitude { get; private set; }

        /// <summary>
        /// Position angle of contact point, in degrees, measured CCW from celestial North pole
        /// </summary>
        public double PAngle { get; private set; }

        /// <summary>
        /// Position angle of contact point, in degrees, measured CCW from zenith
        /// </summary>
        public double ZAngle { get; private set; }
        
        /// <summary>
        /// Parallactic angle, in degrees
        /// </summary>
        public double QAngle { get; private set; }

        /// <summary>
        /// Creates new instance
        /// </summary>        
        public SolarEclipseLocalCircumstancesContactPoint(double jd, double solarAlt, double pAngle, double zAngle, double qAngle)
        {
            JulianDay = jd;
            SolarAltitude = solarAlt;
            PAngle = pAngle;
            ZAngle = zAngle;
            QAngle = qAngle;
        }
    }
}
