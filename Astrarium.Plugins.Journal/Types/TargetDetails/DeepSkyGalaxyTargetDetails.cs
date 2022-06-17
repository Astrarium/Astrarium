using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    public class DeepSkyGalaxyTargetDetails : DeepSkyTargetDetails
    {
        /// <summary>
        /// Position angle, in degrees
        /// </summary>
        public int? PositionAngle { get; set; }

        /// <summary>
        /// Hubble type of galaxy
        /// </summary>
        public string HubbleType { get; set; }
    }
}
