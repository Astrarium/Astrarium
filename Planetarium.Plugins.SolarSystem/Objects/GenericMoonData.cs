using ADK;
using Newtonsoft.Json;
using Planetarium.Plugins.SolarSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Objects
{
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

        public bool jpl { get; set; }
    }
}
