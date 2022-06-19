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

        public string ComparisonStars
        {
            get => GetValue<string>(nameof(ComparisonStars));
            set => SetValue(nameof(ComparisonStars), value);
        }

        /// <summary>
        /// AAVSO chart identifier
        /// </summary>
        public string ChartDate
        {
            get => GetValue<string>(nameof(ChartDate));
            set => SetValue(nameof(ChartDate), value);
        }

        /// <summary>
        /// Flag indicating Non-AAVSO chart ID is provided
        /// </summary>
        public bool? NonAAVSOChart
        {
            get => GetValue<bool?>(nameof(NonAAVSOChart));
            set => SetValue(nameof(NonAAVSOChart), value);
        }

        /// <summary>
        /// The sky is bright (Moon, twilight, light pollution, aurora)
        /// </summary>
        public bool? BrightSky
        {
            get => GetValue<bool?>(nameof(BrightSky));
            set => SetValue(nameof(BrightSky), value);
        }

        public bool? Clouds
        {
            get => GetValue<bool?>(nameof(Clouds));
            set => SetValue(nameof(Clouds), value);
        }

        public bool? PoorSeeing
        {
            get => GetValue<bool?>(nameof(PoorSeeing));
            set => SetValue(nameof(PoorSeeing), value);
        }

        public bool? NearHorizion
        {
            get => GetValue<bool?>(nameof(NearHorizion));
            set => SetValue(nameof(NearHorizion), value);
        }

        public bool? UnusualActivity
        {
            get => GetValue<bool?>(nameof(UnusualActivity));
            set => SetValue(nameof(UnusualActivity), value);
        }

        public bool? Outburst
        {
            get => GetValue<bool?>(nameof(Outburst));
            set => SetValue(nameof(Outburst), value);
        }

        public bool? ComparismSequenceProblem
        {
            get => GetValue<bool?>(nameof(ComparismSequenceProblem));
            set => SetValue(nameof(ComparismSequenceProblem), value);
        }

        public bool? StarIdentificationUncertain
        {
            get => GetValue<bool?>(nameof(StarIdentificationUncertain));
            set => SetValue(nameof(StarIdentificationUncertain), value);
        }

        public bool? FaintStar
        {
            get => GetValue<bool?>(nameof(FaintStar));
            set => SetValue(nameof(FaintStar), value);
        }
    }
}
