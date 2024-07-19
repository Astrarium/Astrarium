using Astrarium.Algorithms;
using Astrarium.Types;

namespace Astrarium.Plugins.SolarSystem.Objects
{
    public class Sun : SizeableCelestialObject, ISolarSystemObject, IMovingObject, IObservableObject
    {
        /// <inheritdoc />
        public override string Type => "Sun";

        /// <inheritdoc />
        public override string CommonName => "Sun";

        /// <summary>
        /// Ecliptical coordinates
        /// </summary>
        public CrdsEcliptical Ecliptical { get; set; }

        /// <summary>
        /// Heliographical coordinates of the solar disk center
        /// </summary>
        public CrdsHeliographical CenterDisk { get; set; }

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

        /// <summary>
        /// Name of the setting(s) responsible for displaying the object
        /// </summary>
        public override string[] DisplaySettingNames => new[] { "Sun" };
    }
}
