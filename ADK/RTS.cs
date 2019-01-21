using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK
{
    public class RTS
    {
        public double Rise { get; set; } = None;
        public double Transit { get; set; } = None;
        public double Set { get; set; } = None;

        public const double None = double.NaN;
        public const double NonRising = double.NegativeInfinity;
        public const double NonSetting = double.PositiveInfinity;
    }
}
