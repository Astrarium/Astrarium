using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    [CelestialObjectType("DeepSky.PlanetaryNebula")]
    public class DeepSkyPlanetaryNebulaTargetDetails : DeepSkyTargetDetails
    {
        /// <summary>
        /// Magnitude of central star
        /// </summary>
        public double? CentralStarMagnitude { get; set; }
    }
}
