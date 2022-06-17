using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    public class VariableStarObservationDetails
    {
        public string ChartDate { get; set; }
        public bool? NonAAVSOChart { get; set; }
        public string ComparisonStars { get; set; }

        public bool? BrightSky { get; set; }
        public bool? Clouds { get; set; }
        public bool? PoorSeeing { get; set; }
        public bool? NearHorizion { get; set; }
        public bool? UnusualActivity { get; set; }
        public bool? Outburst { get; set; }
        public bool? ComparismSequenceProblem { get; set; }
        public bool? StarIdentificationUncertain { get; set; }
        public bool? FaintStar { get; set; }
    }
}
