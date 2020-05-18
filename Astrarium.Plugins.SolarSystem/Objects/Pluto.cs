using Astrarium.Algorithms;
using Astrarium.Types;

namespace Astrarium.Plugins.SolarSystem.Objects
{
    /// <summary>
    /// Contains coordinates and visual appearance data for dwarf planet Pluto.
    /// </summary>
    public class Pluto : SizeableCelestialObject, ISolarSystemObject, IPlanet, IMovingObject
    {
        /// <summary>
        /// Pluto name
        /// </summary>
        public string Name => Text.Get($"Planet.9.Name");

        /// <summary>
        /// Apparent topocentrical equatorial coordinates
        /// </summary>
        public CrdsEquatorial Equatorial { get; set; }

        /// <summary>
        /// Magnitude of planet
        /// </summary>
        public float Magnitude { get; set; }

        /// <summary>
        /// Distance from Earth, in AU
        /// </summary>
        public double DistanceFromEarth { get; set; }

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
        public double AverageDailyMotion => 0.003;

        public int Number => 9;

        public CrdsEcliptical Ecliptical { get; set; }

        public float Flattening => 0;

        public double Elongation { get; set; }

        public double Phase { get; set; }

        /// <summary>
        /// Name of the setting(s) responsible for displaying the object
        /// </summary>
        public override string[] DisplaySettingNames => new[] { "Planets" };
    }
}
