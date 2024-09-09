using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    public abstract class DeepSkyTargetDetails : TargetDetails
    {
        [Ephemeris("SmallDiameter")]
        public double? SmallDiameter { get; set; }

        [Ephemeris("LargeDiameter")]
        public double? LargeDiameter { get; set; }

        [Ephemeris("Magnitude")]
        public double? Magnitude { get; set; }

        [Ephemeris("SurfaceBrightness")]
        public double? Brightness { get; set; }
    }
}
