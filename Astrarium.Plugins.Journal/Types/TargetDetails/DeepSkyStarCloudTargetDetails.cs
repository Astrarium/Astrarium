using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    [CelestialObjectType("DeepSky.StarCloud")]
    public class DeepSkyStarCloudTargetDetails : DeepSkyTargetDetails
    {
        /// <summary>
        /// Position angle of axis, in degrees
        /// </summary>
        public int? PositionAngle { get; set; }
    }
}
