using Astrarium.Algorithms;
using Astrarium.Types;

namespace Astrarium.Plugins.SolarSystem.Objects
{
    public class GenericMoon : SizeableCelestialObject, IPlanetMoon, ISolarSystemObject, IMovingObject
    {
        /// <summary>
        /// Contains data about orbit and main physical characteristics of the satellite
        /// </summary>
        public GenericMoonData Data { get; set; }

        /// <summary>
        /// Moon number, 1-based
        /// </summary>
        public int Number => Data.satellite;

        /// <summary>
        /// Planet number, 1-based
        /// </summary>
        public int Planet => Data.planet;

        /// <summary>
        /// Moon id, used internally for calculation
        /// </summary>
        public int Id => Data.planet * 100 + Data.satellite + (Data.jpl ? 1000 : 0);

        /// <summary>
        /// Apparent equatorial coordinates of the moon
        /// </summary>
        public CrdsEquatorial Equatorial { get; set; }

        /// <summary>
        /// Planetocentric rectangular coordinates of the moon
        /// </summary>
        public CrdsRectangular Rectangular { get; set; }

        /// <summary>
        /// Longitude of central meridian, not used
        /// </summary>
        public double CM => 0;

        /// <summary>
        /// Apparent magnitude
        /// </summary>
        public float Magnitude { get; internal set; }

        /// <summary>
        /// Name of the moon
        /// </summary>
        public string Name => Text.Get(Data.names);

        /// <summary>
        /// Gets moon names
        /// </summary>
        public override string[] Names => new[] { Name };

        public double DistanceFromEarth { get; internal set; }

        public bool IsEclipsedByPlanet => false;

        public double AverageDailyMotion => 0.006668;

        /// <summary>
        /// Name of the setting(s) responsible for displaying the object
        /// </summary>
        public override string[] DisplaySettingNames => new[] { "Planets", "GenericMoons", "PlanetMoons" };
    }
}
