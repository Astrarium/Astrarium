using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    [CelestialObjectType("DeepSky.GalaxyCluster")]
    public class DeepSkyClusterOfGalaxiesTargetDetails : DeepSkyTargetDetails
    {
        /// <summary>
        /// Magnitude of the 10th brightest member in [mag] 
        /// </summary>
        public double? Mag10 { get; set; }
    }
}
