using ADK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Objects
{
    public class Asteroid : SizeableCelestialObject
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
        ///  Absolute magnitude
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
    }
}
