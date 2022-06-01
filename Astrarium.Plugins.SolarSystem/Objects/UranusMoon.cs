using Astrarium.Algorithms;
using Astrarium.Types;

namespace Astrarium.Plugins.SolarSystem.Objects
{
    /// <summary>
    /// Contains coordinates and visual appearance data for the large moon of Uranus for given instant of time.
    /// </summary>
    public class UranusMoon : SizeableCelestialObject, IPlanetMoon, ISolarSystemObject
    {
        public UranusMoon(int number)
        {
            Number = number;
        }

        /// <inheritdoc />
        public override string Type => "PlanetMoon";

        /// <summary>
        /// Common names of all Uranus moons
        /// </summary>
        private static readonly string[] CommonNames = new string[] { "Miranda", "Ariel", "Umbriel", "Titania", "Oberon" };

        /// <inheritdoc />
        public override string CommonName => CommonNames[Number - 1];

        /// <summary>
        /// Moon index, 1-based
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// Apparent equatorial coordinates of the moon
        /// </summary>
        public CrdsEquatorial Equatorial { get; set; }

        /// <summary>
        /// Planetocentric rectangular coordinates of the moon
        /// </summary>
        public CrdsRectangular Rectangular { get; set; }

        /// <summary>
        /// Longitude of central meridian
        /// </summary>
        public double CM { get; internal set; }

        /// <summary>
        /// Apparent magnitude
        /// </summary>
        public float Magnitude { get; internal set; }

        /// <summary>
        /// Name of the moon
        /// </summary>
        public string Name => Text.Get($"UranusMoon.{Number}.Name");

        /// <summary>
        /// Gets moon names
        /// </summary>
        public override string[] Names => new[] { Name };

        public double DistanceFromEarth { get; internal set; }

        public bool IsEclipsedByPlanet => false;

        /// <summary>
        /// Name of the setting(s) responsible for displaying the object
        /// </summary>
        public override string[] DisplaySettingNames => new[] { "Planets", "PlanetMoons" };
    }
}
