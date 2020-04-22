using Astrarium.Algorithms;
using Astrarium.Objects;
using Astrarium.Types;

namespace Astrarium.Plugins.SolarSystem.Objects
{
    public class Sun : SizeableCelestialObject, ISolarSystemObject, IMovingObject
    {
        /// <summary>
        /// Apparent topocentrical equatorial coordinates
        /// </summary>
        public CrdsEquatorial Equatorial { get; set; }

        /// <summary>
        /// Ecliptical coordinates
        /// </summary>
        public CrdsEcliptical Ecliptical { get; set; }

        /// <summary>
        /// Average daily motion of the Sun
        /// </summary>
        public double AverageDailyMotion => 0.985555;

        /// <summary>
        /// Gets Sun names
        /// </summary>
        public override string[] Names => new[] { Name };

        /// <summary>
        /// Distance from Earth
        /// </summary>
        public double DistanceFromEarth => Ecliptical.Distance; 

        /// <summary>
        /// Primary name
        /// </summary>
        public string Name => Text.Get("Sun.Name");
    }
}
