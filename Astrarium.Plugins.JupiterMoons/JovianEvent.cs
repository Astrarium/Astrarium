namespace Astrarium.Plugins.JupiterMoons
{
    /// <summary>
    /// Represent event in Jovian moons system.
    /// </summary>
    public class JovianEvent
    {
        /// <summary>
        /// Julian ephemeris day of beginning of the event.
        /// </summary>
        public double JdBegin { get; set; }

        /// <summary>
        /// Julian ephemeris day of end of the event.
        /// </summary>
        public double JdEnd { get; set; }

        /// <summary>
        /// Event duration, in fractions of day.
        /// </summary>
        public double Duration => JdEnd - JdBegin;

        /// <summary>
        /// Number of Galilean moon, 1 = Io, 2 = Europe, 3 = Ganymede, 4 = Callisto
        /// </summary>
        public int MoonNumber { get; set; }

        /// <summary>
        /// Event code.
        /// </summary>
        /// <example>
        /// Event code examples:
        /// O1 = Occultation of Io
        /// E1 = Eclipse of Io
        /// 2O1 = Europa occults Io
        /// 2E1 = Europa eclipses Io
        /// </example>
        public string Code { get; set; }

        /// <summary>
        /// Textual description of the event.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Altitude of the Sun at beginning of the event.
        /// </summary>
        public double SunAltBegin { get; set; }

        /// <summary>
        /// Altitude of the Sun at end of the event.
        /// </summary>
        public double SunAltEnd { get; set; }

        /// <summary>
        /// Altitude of the Jupiter at beginning of the event.
        /// </summary>
        public double JupiterAltBegin { get; set; }

        /// <summary>
        /// Altitude of the Jupiter at end of the event.
        /// </summary>
        public double JupiterAltEnd { get; set; }

        /// <summary>
        /// Is the Jovian moon eclipsed by Jupiter at beginning of the event.
        /// </summary>
        public bool IsEclipsedAtBegin { get; set; }

        /// <summary>
        /// Is the Jovian moon eclipsed by Jupiter at end of the event.
        /// </summary>
        public bool IsEclipsedAtEnd { get; set; }

        /// <summary>
        /// Is the Jovian moon occulted by Jupiter at beginning of the event.
        /// </summary>
        public bool IsOccultedAtBegin { get; set; }

        /// <summary>
        /// Is the Jovian moon occulted by Jupiter at end of the event.
        /// </summary>
        public bool IsOccultedAtEnd { get; set; }
    }
}
