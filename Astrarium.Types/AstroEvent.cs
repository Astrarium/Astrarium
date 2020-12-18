namespace Astrarium.Types
{
    /// <summary>
    /// Contains information about astronomical phenomena
    /// </summary>
    public class AstroEvent
    {
        /// <summary>
        /// Textual description of the phenomena, for example "Mars in opposition".
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Julian Ephemeris Day of the phenomena.
        /// </summary>
        public double JulianDay { get; private set; }

        /// <summary>
        /// Flag indicating there is no exact time instant for the phenomena.
        /// In this case <see cref="JulianDay"/> property must be used only for date caclulation, not the time.
        /// The flag is usually true for event which has no exact time instant by definition, 
        /// for example, beginning of planet visibility, or maximum activity of meteor shower.
        /// </summary>
        public bool NoExactTime { get; private set; }

        /// <summary>
        /// Primary celestial body related to the event.
        /// Can be null, for example, for equinoxes, solstices and etc.
        /// </summary>
        public CelestialObject PrimaryBody { get; private set; }

        /// <summary>
        /// Secondary celestial body related to the event.
        /// Can be non-null if astronomical phenomena involves two celestial objects,
        /// for example, mutual conjuction of two planets.
        /// For conjunctions with Sun or oppositions, however, secondary body should not be specified.
        /// </summary>
        public CelestialObject SecondaryBody { get; private set; }

        /// <summary>
        /// Creates new astronomical phenomena
        /// </summary>
        /// <param name="jd">Julian Ephemeris Day of the phenomena.</param>
        /// <param name="text">Textual description of the phenomena, for example "Mars in opposition".</param>
        /// <param name="noExactTime">
        /// Flag indicating there is no exact time instant for the phenomena.
        /// In this case <see cref="JulianDay"/> property must be used only for date caclulation, not the time.
        /// The flag is usually true for event which has no exact time instant by definition, 
        /// for example, beginning of planet visibility, or maximum activity of meteor shower.
        /// </param>
        public AstroEvent(double jd, string text, bool noExactTime = false)
        {
            JulianDay = jd;
            Text = text;
            NoExactTime = noExactTime;
        }

        /// <summary>
        /// Creates new astronomical phenomena
        /// </summary>
        /// <param name="jd">Julian Ephemeris Day of the phenomena.</param>
        /// <param name="text">Textual description of the phenomena, for example "Mars in opposition".</param>
        /// <param name="primaryBody">
        /// Primary celestial body related to the event.
        /// Can be null, for example, for equinoxes, solstices and etc.
        /// </param>
        /// <param name="secondaryBody">
        /// Secondary celestial body related to the event.
        /// Can be non-null if astronomical phenomena involves two celestial objects,
        /// for example, mutual conjuction of two planets.
        /// For conjunctions with Sun or oppositions, however, secondary body should not be specified.
        /// </param>
        public AstroEvent(double jd, string text, CelestialObject primaryBody, CelestialObject secondaryBody = null)
        {
            JulianDay = jd;
            Text = text;
            PrimaryBody = primaryBody;
            SecondaryBody = secondaryBody;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"JD={JulianDay}: {Text}";
        }
    }
}
