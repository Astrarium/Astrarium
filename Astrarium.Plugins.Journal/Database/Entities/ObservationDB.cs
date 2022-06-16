using Astrarium.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Database.Entities
{
    public class ObservationDB : IEntity
    {
        public string Id { get; set; }
        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
        public string TargetId { get; set; }
        public string SessionId { get; set; }

        
        public double? Magnification { get; set; }
        public string Accessories { get; set; }

        public string Result { get; set; }

        /// <summary>
        /// Finding details specific for target type, in JSON form.
        /// </summary>
        public string Details { get; set; }

        public string ScopeId { get; set; }
        public string EyepieceId { get; set; }
        public string LensId { get; set; }
        public string FilterId { get; set; }
        public string ImagerId { get; set; }

        // related entities

        public virtual OpticsDB Scope { get; set; }
        public virtual EyepieceDB Eyepiece { get; set; }
        public virtual LensDB Lens { get; set; }
        public virtual FilterDB Filter { get; set; }
        public virtual TargetDB Target { get; set; }
        public virtual ImagerDB Imager { get; set; }
        public virtual ICollection<AttachmentDB> Attachments { get; set; }
    }

    public class DeepSkyObservationDetails : PropertyChangedBase
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

    public class DoubleStarObservationDetails : DeepSkyObservationDetails
    {
        /// <summary>
        /// Color of main component, string with possible values:
        /// "white", "red", "orange", "yellow", "green", "blue"
        /// </summary>
        public string ColorMainComponent { get; set; }

        /// <summary>
        /// Color of main component, string with possible values:
        /// "white", "red", "orange", "yellow", "green", "blue"
        /// </summary>
        public string ColorCompainionComponent { get; set; }

        public bool? EqualBrightness { get; set; }
        public bool? NiceSurrounding { get; set; }
    }

    public class OpenClusterObservationDetails : DeepSkyObservationDetails
    {
        // TODO: character descriptions

        /// <summary>
        /// Character of the cluster according to "Deep Sky Liste" definition
        /// </summary>
        public string Character
        {
            get => GetValue<string>(nameof(Character));
            set => SetValue(nameof(Character), value);
        }

        public bool? UnusualShape
        {
            get => GetValue<bool?>(nameof(UnusualShape));
            set => SetValue(nameof(UnusualShape), value);
        }

        public bool? PartlyUnresolved
        {
            get => GetValue<bool?>(nameof(PartlyUnresolved));
            set => SetValue(nameof(PartlyUnresolved), value);
        }

        public bool? ColorContrasts
        {
            get => GetValue<bool?>(nameof(ColorContrasts));
            set => SetValue(nameof(ColorContrasts), value);
        }
    }

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
