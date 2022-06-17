using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    public class DeepSkyDoubleStarTargetDetails : DeepSkyTargetDetails
    {
        /// <summary>
        /// Position angle, in degrees
        /// </summary>
        public int? PositionAngle { get; set; }

        /// <summary>
        /// Separation between components
        /// </summary>
        public double? Separation { get; set; }

        /// <summary>
        /// Magnitude of companion star
        /// </summary>
        public double? CompanionMagnitude { get; set; }
    }
}
