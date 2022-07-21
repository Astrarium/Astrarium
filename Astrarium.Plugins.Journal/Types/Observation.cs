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
    public class Observation : DBStoredEntity
    {
        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.None };

        public string Id { get; private set; }

        public Observation(string id)
        {
            Id = id;
        }

        public override DateTime SessionDate => Session.Begin;

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

        [DBStored(Entity = typeof(ObservationDB), Field = "Details")]
        private string details => JsonConvert.SerializeObject(Details, jsonSettings);

        public PropertyChangedBase Details
        {
            get => GetValue<PropertyChangedBase>(nameof(Details));
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

        public object TargetDetails
        {
            get => GetValue<object>(nameof(TargetDetails));
            set => SetValue(nameof(TargetDetails), value);
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

        [DBStored(Entity = typeof(ObservationDB), Field = "ImagerId")]
        public string CameraId
        {
            get => GetValue<string>(nameof(CameraId), null);
            set => SetValue(nameof(CameraId), value);
        }

        public string Constellation
        {
            get => GetValue<string>(nameof(Constellation), null);
            set => SetValue(nameof(Constellation), value);
        }

        public CrdsEquatorial EquatorialCoordinates
        {
            get => GetValue<CrdsEquatorial>(nameof(EquatorialCoordinates), null);
            set => SetValue(nameof(EquatorialCoordinates), value);
        }

        public CrdsHorizontal HorizontalCoordinates
        {
            get => GetValue<CrdsHorizontal>(nameof(HorizontalCoordinates), null);
            set => SetValue(nameof(HorizontalCoordinates), value);
        }
    }
}
