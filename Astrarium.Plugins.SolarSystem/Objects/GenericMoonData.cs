using Astrarium.Algorithms;
using System.Collections.Generic;

namespace Astrarium.Plugins.SolarSystem.Objects
{
    /// <summary>
    /// Represents orbital data for generic planet moon along with moon/planet indices, names and absolute magnitude / radius.
    /// </summary>
    public class GenericMoonData : GenericSatelliteOrbit
    {
        /// <summary>
        /// Satellite number, 1-based 
        /// </summary>
        public int satellite { get; set; }

        /// <summary>
        /// Planet number, 1-based
        /// </summary>
        public int planet { get; set; }

        /// <summary>
        /// Satellite names, key is language code, value is localized name
        /// </summary>
        public Dictionary<string, string> names { get; set; }

        /// <summary>
        /// Absolute magnitude
        /// </summary>
        public double mag { get; set; }

        /// <summary>
        /// Mean radius of satellite, in km
        /// </summary>
        public double radius { get; set; }
    }
}
