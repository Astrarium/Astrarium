using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    [CelestialObjectType("DeepSky.OpenCluster")]
    public class DeepSkyOpenClusterTargetDetails : DeepSkyTargetDetails
    {
        /// <summary>
        /// Number of stars
        /// </summary>
        public int? StarsCount { get; set; }

        /// <summary>
        /// Magnitude of brightest star in [mag]
        /// </summary>
        public double? BrightestStarMagnitude { get; set; }

        /// <summary>
        /// Classification according to Trumpler
        /// </summary>
        [Ephemeris("ObjectType")]
        public string TrumplerClass { get; set; }
    }
}
