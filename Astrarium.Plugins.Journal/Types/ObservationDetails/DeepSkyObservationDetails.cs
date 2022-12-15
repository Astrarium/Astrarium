using Astrarium.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    public class DeepSkyObservationDetails : ObservationDetails
    {
        [JsonIgnore]
        public bool SmallDiameterSpecified
        {
            get => GetValue<bool>(nameof(SmallDiameterSpecified));
            set
            {
                SetValue(nameof(SmallDiameterSpecified), value);
                NotifyPropertyChanged(nameof(SmallDiameter));
            }
        }

        [JsonIgnore]
        public decimal SmallDiameterValue
        {
            get => GetValue<decimal>(nameof(SmallDiameterValue));
            set
            {
                SetValue(nameof(SmallDiameterValue), value);
                NotifyPropertyChanged(nameof(SmallDiameter));
            }
        }

        public double? SmallDiameter
        {
            get => SmallDiameterSpecified ? (double)SmallDiameterValue : (double?)null;
            set
            {
                SmallDiameterSpecified = value != null;
                SmallDiameterValue = SmallDiameterSpecified ? (decimal)value.Value : 0;
            }
        }

        [JsonIgnore]
        public bool LargeDiameterSpecified
        {
            get => GetValue<bool>(nameof(LargeDiameterSpecified));
            set
            {
                SetValue(nameof(LargeDiameterSpecified), value);
                NotifyPropertyChanged(nameof(LargeDiameter));
            }
        }

        [JsonIgnore]
        public decimal LargeDiameterValue
        {
            get => GetValue<decimal>(nameof(LargeDiameterValue));
            set
            {
                SetValue(nameof(LargeDiameterValue), value);
                NotifyPropertyChanged(nameof(LargeDiameter));
            }
        }

        public double? LargeDiameter
        {
            get => LargeDiameterSpecified ? (double)LargeDiameterValue : (double?)null;
            set
            {
                LargeDiameterSpecified = value != null;
                LargeDiameterValue = LargeDiameterSpecified ? (decimal)value.Value : 0;
            }
        }

        public bool? Stellar
        {
            get => GetValue<bool?>(nameof(Stellar));
            set => SetValue(nameof(Stellar), value);
        }

        public bool? Extended
        {
            get => GetValue<bool?>(nameof(Extended));
            set => SetValue(nameof(Extended), value);
        }

        public bool? Resolved
        {
            get => GetValue<bool?>(nameof(Resolved));
            set => SetValue(nameof(Resolved), value);
        }

        public bool? Mottled
        {
            get => GetValue<bool?>(nameof(Mottled));
            set => SetValue(nameof(Mottled), value);
        }

        /// <summary>
        /// Rating according to the scale of the "Deep Sky Liste", 99 means "unknown"
        /// </summary>
        public int Rating
        {
            get => GetValue<int>(nameof(Rating), 99);
            set => SetValue(nameof(Rating), value);
        }
    }
}
