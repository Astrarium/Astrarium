using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.SolarSystem.Objects
{
    internal interface IPlanetMoon
    {
        /// <summary>
        /// Longitude of central meridian
        /// </summary>
        double CM { get; }

        /// <summary>
        /// Name of the moon
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Number of the moon
        /// </summary>
        int Number { get; }

        /// <summary>
        /// Apparent magnitude
        /// </summary>
        float Magnitude { get;  }

        /// <summary>
        /// Flag indicating the moon is eclipsed by parent planet
        /// </summary>
        bool IsEclipsedByPlanet { get; }
    }
}
