using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    public abstract class DeepSkyTargetDetails : TargetDetails
    {
        public double? SmallDiameter { get; set; }
        public double? LargeDiameter { get; set; }
        public double? Magnitude { get; set; }
        public double? Brightness { get; set; }
    }
}
