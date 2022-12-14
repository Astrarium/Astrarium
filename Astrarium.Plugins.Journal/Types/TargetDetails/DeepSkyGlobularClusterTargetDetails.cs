using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    [CelestialObjectType("DeepSky.GlobularCluster")]
    public class DeepSkyGlobularClusterTargetDetails : DeepSkyTargetDetails
    {
        /// <summary>
        /// Magnitude of brightest stars in [mag]
        /// </summary>
        public double? MagStars { get; set; }

        /// <summary>
        /// Degree of concentration [I..XII]
        /// </summary>
        public string Concentration { get; set; }
    }
}
