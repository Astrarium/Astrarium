using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    public class VariableStarObservationDetails : PropertyChangedBase
    {
        /// <summary>
        /// Observed visual magnitude of the variable star
        /// </summary>
        public double VisMag
        {
            get => GetValue<double>(nameof(VisMag), 6);
            set => SetValue(nameof(VisMag), value);
        }

        /// <summary>
        /// Flag indicating visual magnitude is uncertain 
        /// </summary>
        public bool? VisMagUncertain
        {
            get => GetValue<bool?>(nameof(VisMagUncertain));
            set => SetValue(nameof(VisMagUncertain), value);
        }

        /// <summary>
        /// Flag indicating visual magnitude is a fainter than value
        /// </summary>
        public bool? VisMagFainterThan
        {
            get => GetValue<bool?>(nameof(VisMagFainterThan));
            set => SetValue(nameof(VisMagFainterThan), value);
        }

        public string ComparisonStars { get; set; }


        public string ChartDate { get; set; }


        public bool? NonAAVSOChart { get; set; }


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
