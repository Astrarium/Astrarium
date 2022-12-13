using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    public class TargetDetails
    {
        [Ephemeris("Equatorial.Alpha")]
        public double? RA { get; set; }

        [Ephemeris("Equatorial.Delta")]
        public double? Dec { get; set; }

        [Ephemeris("Horizontal.Altitude")]
        public double? Alt { get; set; }

        [Ephemeris("Horizontal.Azimuth")]
        public double? Azi { get; set; }

        [Ephemeris("Constellation")]
        public string Constellation { get; set; }
    }
}
