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
        /// Degree of concentration [I..XII], aka Shapley Sawyer Concentration Class
        /// </summary>
        [Ephemeris("ObjectType")]
        public string Concentration { get; set; }
    }
}
