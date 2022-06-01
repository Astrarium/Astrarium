using Astrarium.Algorithms;
using Astrarium.Types;

namespace Astrarium.Plugins.SolarSystem.Objects
{
    /// <summary>
    /// Contains coordinates and visual appearance data for the major planet for given instant of time.
    /// </summary>
    public class Planet : SizeableCelestialObject, IPlanet, ISolarSystemObject, IMovingObject
    {
        public Planet(int number)
        {
            Number = number;
        }

        /// <summary>
        /// Common names of all Planets
        /// </summary>
        private static readonly string[] CommonNames = new string[] { "Mercury", "Venus", "Earth", "Mars", "Jupiter", "Saturn", "Uranus", "Neptune" };

        /// <inheritdoc />
        public override string Type => "Planet";

        /// <inheritdoc />
        public override string CommonName => CommonNames[Number - 1];

        /// <summary>
        /// Serial number of the planet, from 1 (Mercury) to 8 (Neptune).
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// Planet name
        /// </summary>
        public string Name => Text.Get($"Planet.{Number}.Name");

        /// <summary>
        /// Geocentrical equatorial coordinates
        /// </summary>
        public CrdsEquatorial Equatorial0 { get; set; } = new CrdsEquatorial();

        /// <summary>
        /// Apparent topocentrical equatorial coordinates
        /// </summary>
        public CrdsEquatorial Equatorial { get; set; }

        /// <summary>
        /// Ecliptical coordinates
        /// </summary>
        public CrdsEcliptical Ecliptical { get; set; }

        /// <summary>
        /// Planet flattening. 0 means ideal sphere.
        /// </summary>
        public float Flattening { get; set; }

        /// <summary>
        /// Current elongation angle.
        /// </summary>
        public double Elongation { get; set; }

        /// <summary>
        /// Current phas of the planet.
        /// </summary>
        public double Phase { get; set; }

        /// <summary>
        /// Magnitude of planet
        /// </summary>
        public float Magnitude { get; set; }

        /// <summary>
        /// Distance from Sun, in AU
        /// </summary>
        public double DistanceFromSun { get; set; }

        /// <summary>
        /// Distance from Earth, in AU
        /// </summary>
        public double DistanceFromEarth => Ecliptical.Distance;

        /// <summary>
        /// Gets planet names
        /// </summary>
        public override string[] Names => new[] { Name };

        /// <summary>
        /// Planet appearance parameters
        /// </summary>
        public PlanetAppearance Appearance { get; set; }

        /// <summary>
        /// Mean daily motion of the planet, in degrees
        /// </summary>
        public double AverageDailyMotion => DAILY_MOTIONS[Number - 1];

        /// <summary>
        /// Average daily motions of planets
        /// </summary>
        public static readonly double[] DAILY_MOTIONS = new[] { 1.3833, 1.2, 0, 0.542, 0.0831, 0.0336, 0.026666, 0.006668 };

        public const int MERCURY    = 1;
        public const int VENUS      = 2;
        public const int EARTH      = 3;
        public const int MARS       = 4;
        public const int JUPITER    = 5;
        public const int SATURN     = 6;
        public const int URANUS     = 7;
        public const int NEPTUNE    = 8;
        public const int PLUTO      = 9;

        public static readonly string[] NAMES =
        {
            "MERCURY",
            "VENUS",
            "EARTH",
            "MARS",
            "JUPITER",
            "SATURN",
            "URANUS",
            "NEPTUNE",
            "PLUTO"
        };

        /// <summary>
        /// Name of the setting(s) responsible for displaying the object
        /// </summary>
        public override string[] DisplaySettingNames => new[] { "Planets" };
    }
}
