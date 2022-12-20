using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    [CelestialObjectType("DeepSky.GalacticNebula")]
    public class DeepSkyGalaxyNebulaTargetDetails : DeepSkyTargetDetails
    {
        /// <summary>
        /// Position angle, in degrees
        /// </summary>
        [Ephemeris("PositionAngle")]
        public int? PositionAngle { get; set; }

        /// <summary>
        /// Indicates emission, reflection or dark nebula not restricted to an enum to cover exotic objects
        /// </summary>
        [Ephemeris("ObjectType")]
        public string NebulaType { get; set; }
    }
}
