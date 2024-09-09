using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    [CelestialObjectType("DeepSky.Galaxy")]
    public class DeepSkyGalaxyTargetDetails : DeepSkyTargetDetails
    {
        /// <summary>
        /// Position angle, in degrees
        /// </summary>
        [Ephemeris("PositionAngle")]
        public int? PositionAngle { get; set; }

        /// <summary>
        /// Hubble type of galaxy
        /// </summary>
        [Ephemeris("ObjectType")]
        public string HubbleType { get; set; }
    }
}
