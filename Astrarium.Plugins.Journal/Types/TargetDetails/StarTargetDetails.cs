using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    [CelestialObjectType("Star")]
    public class StarTargetDetails : TargetDetails
    {
        [Ephemeris("Magnitude")]
        public double? Magnitude { get; set; }

        [Ephemeris("SpectralClass")]
        public string Classification { get; set; }
    }
}
