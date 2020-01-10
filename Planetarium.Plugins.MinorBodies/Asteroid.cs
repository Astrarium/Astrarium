using ADK;
using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.MinorBodies
{
    public class Asteroid : SizeableCelestialObject, IMovingObject
    {
        /// <summary>
        /// Name or readable designation of the minor planet
        /// </summary>
        public string Name { get; set; }

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
    }
}
