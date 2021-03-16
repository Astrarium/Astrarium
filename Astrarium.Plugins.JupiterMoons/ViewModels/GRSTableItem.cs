using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.JupiterMoons
{
    public class GRSTableItem
    {
        public GRSEvent Event { get; set; }
        public string Date { get; set; }
        public string AppearTime { get; set; }
        public string TransitTime { get; set; }
        public string DisappearTime { get; set; }
        public string JupiterAltTransit { get; set; }
        public string SunAltTransit { get; set; }
        public string JupiterAltAppear { get; set; }
        public string SunAltAppear { get; set; }
        public string JupiterAltDisappear { get; set; }
        public string SunAltDisappear { get; set; }
    }
}
