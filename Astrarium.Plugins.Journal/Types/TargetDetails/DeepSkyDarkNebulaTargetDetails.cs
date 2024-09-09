using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    [CelestialObjectType("DeepSky.DarkNebula")]
    public class DeepSkyDarkNebulaTargetDetails : DeepSkyTargetDetails
    {
        /// <summary>
        /// Position angle of axis, in degrees
        /// </summary>
        [Ephemeris("PositionAngle")]
        public int? PositionAngle { get; set; }

        /// <summary>
        /// Opacity acc. to Lynds (1: min, 6: max)
        /// </summary>
        public int? Opacity { get; set; }
    }
}
