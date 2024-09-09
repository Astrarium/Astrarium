using Astrarium.Algorithms;
using Astrarium.Plugins.Journal.Database.Entities;
using Astrarium.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    public class Observation : JournalEntity
    {
        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.None };

        public string Id { get; private set; }

        public Observation(string id)
        {
            Id = id;
        }

        public override DateTime SessionDate => Session.Begin.Date;

        /// <summary>
        /// Session instance associated with the observation
        /// </summary>
        public Session Session { get; set; }

        /// <summary>
        /// Required by the tree view control
        /// </summary>
        public bool IsExpanded { get => false; set { } }

        public string ObjectName
        {
            get => GetValue<string>(nameof(ObjectName));
            set => SetValue(nameof(ObjectName), value);
        }

        public string ObjectCommonName
        {
            get => GetValue<string>(nameof(ObjectCommonName));
            set => SetValue(nameof(ObjectCommonName), value);
        }

        public string ObjectNameAliases
        {
            get => GetValue<string>(nameof(ObjectNameAliases));
            set => SetValue(nameof(ObjectNameAliases), value);
        }

        public string ObjectType
        {
            get => GetValue<string>(nameof(ObjectType));
            set => SetValue(nameof(ObjectType), value);
        }

        [DBStored(Entity = typeof(ObservationDB), Field = "Result")]
        public string Findings
        {
            get => GetValue<string>(nameof(Findings));
            set => SetValue(nameof(Findings), value);
        }

        [DBStored(Entity = typeof(ObservationDB), Field = "Accessories")]
        public string Accessories
        {
            get => GetValue<string>(nameof(Accessories));
            set => SetValue(nameof(Accessories), value);
        }

        [DBStored(Entity = typeof(ObservationDB), Field = "Details")]
        private string details => JsonConvert.SerializeObject(Details, jsonSettings);

        public ObservationDetails Details
        {
            get => GetValue<ObservationDetails>(nameof(Details));
            set
            {
                var old = Details;
                if (old != null)
                {
                    old.PropertyChanged -= DetailsPropertyChanged;
                }
                if (value != null)
                {
                    value.PropertyChanged += DetailsPropertyChanged;
                }
                SetValue(nameof(Details), value);
            }
        }

        private void DetailsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(details));
        }

        public TargetDetails TargetDetails
        {
            get => GetValue<TargetDetails>(nameof(TargetDetails));
            set
            {
                SetValue(nameof(TargetDetails), value);
                NotifyPropertyChanged(
                    nameof(EquatorialCoordinates),
                    nameof(Constellation));
            }
        }

        public string TargetId
        {
            get => GetValue<string>(nameof(TargetId));
            set => SetValue(nameof(TargetId), value);
        }

        [DBStored(Entity = typeof(TargetDB), Field = "Notes", Key = "TargetId")]
        public string TargetNotes
        {
            get => GetValue<string>(nameof(TargetNotes));
            set => SetValue(nameof(TargetNotes), value);
        }

        [DBStored(Entity = typeof(ObservationDB), Field = "ScopeId")]
        public string TelescopeId
        {
            get => GetValue<string>(nameof(TelescopeId), null);
            set => SetValue(nameof(TelescopeId), value);
        }

        [DBStored(Entity = typeof(ObservationDB), Field = "EyepieceId")]
        public string EyepieceId
        {
            get => GetValue<string>(nameof(EyepieceId), null);
            set => SetValue(nameof(EyepieceId), value);
        }

        [DBStored(Entity = typeof(ObservationDB), Field = "LensId")]
        public string LensId
        {
            get => GetValue<string>(nameof(LensId), null);
            set => SetValue(nameof(LensId), value);
        }

        [DBStored(Entity = typeof(ObservationDB), Field = "FilterId")]
        public string FilterId
        {
            get => GetValue<string>(nameof(FilterId), null);
            set => SetValue(nameof(FilterId), value);
        }

        [DBStored(Entity = typeof(ObservationDB), Field = "CameraId")]
        public string CameraId
        {
            get => GetValue<string>(nameof(CameraId), null);
            set => SetValue(nameof(CameraId), value);
        }

        public string Constellation
        {
            get => TargetDetails != null ? TargetDetails.Constellation : null;
        }

        public CrdsEquatorial EquatorialCoordinates
        {
            get
            {
                if (TargetDetails != null && TargetDetails.RA != null && TargetDetails.Dec != null)
                {
                    return new CrdsEquatorial(TargetDetails.RA.Value, TargetDetails.Dec.Value);
                }
                return null;
            }
        }
    }
}
