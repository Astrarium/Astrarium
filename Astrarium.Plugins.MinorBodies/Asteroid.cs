using Astrarium.Algorithms;
using Astrarium.Types;

namespace Astrarium.Plugins.MinorBodies
{
    public class Asteroid : SizeableCelestialObject, IMovingObject
    {
        /// <summary>
        /// Name or readable designation of the minor planet
        /// </summary>
        public string Name { get; set; }

        /// <inheritdoc />
        public override string Type => "Asteroid";

        /// <summary>
        /// Orbital elements of the minor planet
        /// </summary>
        public OrbitalElements Orbit { get; set; }

        /// <summary>
        /// Absolute magnitude
        /// </summary>
        public double H { get; set; }

        /// <summary>
        /// Slope parameter
        /// </summary>
        public double G { get; set; }

        /// <summary>
        /// Magnitude of asteroid
        /// </summary>
        public float Magnitude { get; set; }

        /// <summary>
        /// Physical diameter, in km, if available
        /// </summary>
        public float PhysicalDiameter { get; set; }

        /// <summary>
        /// Maximal possible brightness (visual magnitude)
        /// </summary>
        public float? MaxBrightness { get; set; }

        /// <summary>
        /// Average daily motion of asteroid
        /// </summary>
        public double AverageDailyMotion { get; set; }

        /// <summary>
        /// Gets array of asteroid names
        /// </summary>
        public override string[] Names => new[] { Name };

        /// <summary>
        /// Name of the setting(s) responsible for displaying the object
        /// </summary>
        public override string[] DisplaySettingNames => new[] { "Asteroids" };

        /// <summary>
        /// Creates new instance
        /// </summary>
        public Asteroid()
        {
            Horizontal = new CrdsHorizontal();
        }
    }
}
