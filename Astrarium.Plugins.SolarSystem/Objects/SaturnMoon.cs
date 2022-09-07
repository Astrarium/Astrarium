using Astrarium.Algorithms;
using Astrarium.Types;

namespace Astrarium.Plugins.SolarSystem.Objects
{
    /// <summary>
    /// Contains coordinates and visual appearance data for the major moon of Saturn for given instant of time.
    /// </summary>
    public class SaturnMoon : SizeableCelestialObject, IPlanetMoon, ISolarSystemObject, IMagnitudeObject
    {
        public SaturnMoon(int number)
        {
            Number = number;
        }

        /// <inheritdoc />
        public override string Type => "PlanetMoon";

        /// <summary>
        /// Common names of all Saturn moons
        /// </summary>
        private static readonly string[] CommonNames = new string[] { "Mimas", "Enceladus", "Tethys", "Dione", "Rhea", "Titan", "Hyperion", "Japetus" };

        /// <inheritdoc />
        public override string CommonName => CommonNames[Number - 1];

        /// <summary>
        /// Apparent equatorial coordinates of the Saturn moon
        /// </summary>
        public CrdsEquatorial Equatorial { get; internal set; }

        /// <summary>
        /// Planetocentric rectangular coordinates of the Saturn moon
        /// </summary>
        public CrdsRectangular Rectangular { get; internal set; }

        /// <summary>
        /// Name of the Saturn moon
        /// </summary>
        public string Name => Text.Get($"SaturnMoon.{Number}.Name");

        /// <summary>
        /// Gets Saturn moon names
        /// </summary>
        public override string[] Names => new[] { Name };

        public double DistanceFromEarth { get; internal set; }

        /// <summary>
        /// Number of the moon
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// Longitude of central meridian
        /// </summary>
        public double CM { get; internal set; }

        /// <summary>
        /// Apparent magnitude
        /// </summary>
        public float Magnitude { get; internal set; }

        public bool IsEclipsedByPlanet => false;

        /// <summary>
        /// Name of the setting(s) responsible for displaying the object
        /// </summary>
        public override string[] DisplaySettingNames => new[] { "Planets", "PlanetMoons" };
    }
}
