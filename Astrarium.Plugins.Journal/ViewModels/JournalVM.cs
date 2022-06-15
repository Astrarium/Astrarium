using Astrarium.Algorithms;
using Astrarium.Plugins.Journal.Database;
using Astrarium.Plugins.Journal.Database.Entities;
using Astrarium.Types;
using Astrarium.Types.Themes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Astrarium.Plugins.Journal.ViewModels
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DBStoredAttribute : Attribute
    {
        public Type Entity { get; set; }
        public string Key { get; set; }
        public string Field { get; set; }
    }

    public abstract class DBStoredEntity : PropertyChangedBase
    {
        public event Action<object, Type, string, object> DatabasePropertyChanged;

        protected override void NotifyPropertyChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                var prop = GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var attr = prop.GetCustomAttribute<DBStoredAttribute>();
                if (attr != null)
                {
                    var keyProp = GetType().GetProperty(attr.Key ?? "Id");
                    var key = keyProp.GetValue(this);

                    DatabasePropertyChanged?.Invoke(prop.GetValue(this), attr.Entity, attr.Field, key);
                }
            }
           
            base.NotifyPropertyChanged(propertyNames);
        }
    }

    public class TreeItemSession : DBStoredEntity
    {
        public string Id { get; private set; }

        public TreeItemSession(string id)
        {
            Id = id;
        }

        public bool IsExpanded
        {
            get => GetValue(nameof(IsExpanded), false);
            set => SetValue(nameof(IsExpanded), value);
        }

        public string Date
        {
            get => GetValue<string>(nameof(Date));
            set => SetValue(nameof(Date), value);
        }

        public string Time
        {
            get => GetValue<string>(nameof(Time));
            set => SetValue(nameof(Time), value);
        }

        [DBStored(Entity = typeof(SessionDB), Field = "Comments")]
        public string Comments
        {
            get => GetValue<string>(nameof(Comments), null);
            set => SetValue(nameof(Comments), value);
        }

        [DBStored(Entity = typeof(SessionDB), Field = "Weather")]
        public string Weather
        {
            get => GetValue<string>(nameof(Weather), null);
            set => SetValue(nameof(Weather), value);
        }

        [DBStored(Entity = typeof(SessionDB), Field = "Seeing")]
        public int? Seeing
        {
            get => GetValue<int?>(nameof(Seeing), null);
            set => SetValue(nameof(Seeing), value);
        }

        public bool SkyQualitySpecified
        {
            get => GetValue(nameof(SkyQualitySpecified), false);
            set
            {
                SetValue(nameof(SkyQualitySpecified), value);
                NotifyPropertyChanged(nameof(skyQuality));
            }
        }

        /// <summary>
        /// This used only for DB storing
        /// </summary>
        [DBStored(Entity = typeof(SessionDB), Field = "FaintestStar")]
        private double? faintestStar => FaintestStarSpecified ? (double)FaintestStar : (double?)null;

        public decimal FaintestStar
        {
            get => GetValue(nameof(FaintestStar), 6.0m);
            set
            {
                SetValue(nameof(FaintestStar), value);
                NotifyPropertyChanged(nameof(faintestStar));
            }
        }

        public bool FaintestStarSpecified
        {
            get => GetValue(nameof(FaintestStarSpecified), false);
            set 
            { 
                SetValue(nameof(FaintestStarSpecified), value);
                NotifyPropertyChanged(nameof(faintestStar));
            }
        }

        /// <summary>
        /// This used only for DB storing
        /// </summary>
        [DBStored(Entity = typeof(SessionDB), Field = "SkyQuality")]
        private double? skyQuality => SkyQualitySpecified ? (double)SkyQuality : (double?)null;

        public decimal SkyQuality
        {
            get => GetValue(nameof(SkyQuality), 19m);
            set 
            { 
                SetValue(nameof(SkyQuality), value);
                NotifyPropertyChanged(nameof(skyQuality));
            }
        }

        [DBStored(Entity = typeof(SessionDB), Field = "Equipment")]
        public string Equipment
        {
            get => GetValue<string>(nameof(Equipment), null);
            set => SetValue(nameof(Equipment), value);
        }

        public int ObservationsCount
        {
            get => Observations.Count;
        }

        public ObservableCollection<TreeItemObservation> Observations { get; private set; } = new ObservableCollection<TreeItemObservation>();
    }

    public class TreeItemObservation : DBStoredEntity
    {
        public string Id { get; private set; }

        public TreeItemObservation(string id)
        {
            Id = id;
        }

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
        private string details => JsonConvert.SerializeObject(Details, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.None });

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

        public string Eyepiece
        {
            get => GetValue<string>(nameof(Eyepiece), null);
            set => SetValue(nameof(Eyepiece), value);
        }

        public string Lens
        {
            get => GetValue<string>(nameof(Lens), null);
            set => SetValue(nameof(Lens), value);
        }

        public string Camera
        {
            get => GetValue<string>(nameof(Camera), null);
            set => SetValue(nameof(Camera), value);
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

    public class JournalVM : ViewModelBase
    {
        public ICommand ExpandCollapseCommand { get; private set; }

        public ObservableCollection<TreeItemSession> AllSessions { get; private set; } = new ObservableCollection<TreeItemSession>();

        public JournalVM()
        {
            ExpandCollapseCommand = new Command(ExpandCollapse);
        }

        private void ExpandCollapse()
        {
            if (AllSessions.Any(x => !x.IsExpanded))
            {
                foreach (var s in AllSessions)
                {
                    s.IsExpanded = true;
                }
            }
            else
            {
                foreach (var s in AllSessions)
                {
                    s.IsExpanded = false;
                }
            }
        }

        public DBStoredEntity SelectedTreeViewItem
        {
            get => GetValue<DBStoredEntity>(nameof(SelectedTreeViewItem));
            set
            {
                if (SelectedTreeViewItem != null)
                {
                    SelectedTreeViewItem.DatabasePropertyChanged -= SaveDatabaseEntityProperty;
                }

                SetValue(nameof(SelectedTreeViewItem), value);
                LoadSelectedTreeViewItemDetails();

                if (value != null)
                {
                    value.DatabasePropertyChanged += SaveDatabaseEntityProperty;
                }
            }
        }

        private void SaveDatabaseEntityProperty(object value, Type entityType, string column, object key)
        {
            using (var db = new DatabaseContext())
            {
                var entity = db.Set(entityType).Find(key);
                db.Entry(entity).Property(column).CurrentValue = value;
                db.SaveChanges();
            }
        }

        public async void Load()
        {
            await Task.Run(() =>
            {
                using (var db = new DatabaseContext())
                {
                    var sessions = db.Sessions
                        .Include(x => x.Observations)
                        .Include(x => x.Observations.Select(o => o.Target))
                        .OrderByDescending(x => x.Begin).ToArray();

                    List<TreeItemSession> allSessions = new List<TreeItemSession>();

                    foreach (var s in sessions)
                    {
                        var item = new TreeItemSession(s.Id)
                        {
                            Date = s.Begin.ToString("dd MMM yyyy"),
                            Time = $"{s.Begin.ToString("HH:mm")}-{s.End.ToString("HH:mm")}"
                        };

                        var observations = s.Observations.OrderByDescending(x => x.Begin);
                        foreach (var obs in observations)
                        {
                            item.Observations.Add(new TreeItemObservation(obs.Id)
                            {
                                ObjectName = obs.Target.Name,
                                ObjectType = obs.Target.Type,
                                ObjectNameAliases = DeserializeAliases(obs.Target.Aliases)
                            });
                        }

                        allSessions.Add(item);
                    }

                    AllSessions = new ObservableCollection<TreeItemSession>(allSessions);
                    NotifyPropertyChanged(nameof(AllSessions));
                }
            });
        }

        private void LoadSelectedTreeViewItemDetails()
        {
            if (SelectedTreeViewItem is TreeItemSession session)
            {
                using (var db = new DatabaseContext())
                {
                    var s = db.Sessions.FirstOrDefault(x => x.Id == session.Id);
                    session.Weather = s.Weather;
                    session.Seeing = s.Seeing;
                    session.FaintestStar = s.FaintestStar != null ? (decimal)s.FaintestStar.Value : 6m;
                    session.FaintestStarSpecified = s.FaintestStar != null;
                    session.SkyQuality = s.SkyQuality != null ? (decimal)s.SkyQuality.Value : 19m;
                    session.SkyQualitySpecified = s.SkyQuality != null;

                    session.Equipment = s.Equipment;
                    session.Comments = s.Comments;
                }
            }
            else if (SelectedTreeViewItem is TreeItemObservation observation)
            {
                using (var db = new DatabaseContext())
                {
                    var obs = db.Observations
                        .Include(x => x.Target)
                        .Include(x => x.Eyepiece)
                        .Include(x => x.Lens)
                        .Include(x => x.Imager)
                        .FirstOrDefault(x => x.Id == observation.Id);

                    observation.Findings = obs.Result;
                    observation.Details = DeserializeObservationDetails(obs.Target.Type, obs.Details);
                    observation.TargetDetails = DeserializeTargetDetails(obs.Target.Type, obs.Target.Details);

                    observation.TelescopeId = obs.ScopeId;
                    observation.Eyepiece = obs.Eyepiece != null ? (obs.Eyepiece.Vendor + " " + obs.Eyepiece.Model) : null;
                    observation.Lens = obs.Lens != null ? (obs.Lens?.Vendor + " " + obs.Lens.Model) : null;
                    observation.Camera = obs.Imager != null ? (obs.Imager?.Vendor + " " + obs.Imager.Model) : null;

                    observation.Constellation = obs.Target?.Constellation;
                    observation.EquatorialCoordinates = obs.Target?.RightAscension != null && obs.Target?.Declination != null ? new CrdsEquatorial((double)obs.Target?.RightAscension.Value, (double)obs.Target?.Declination.Value) : null;


                }
            }
        }

        private string DeserializeAliases(string aliases)
        {
            if (string.IsNullOrEmpty(aliases))
                return null;

            string value = string.Join(", ", JsonConvert.DeserializeObject<string[]>(aliases));
            if (string.IsNullOrEmpty(value))
                return null;
            else
                return value;
        }

        private PropertyChangedBase DeserializeObservationDetails(string targetType, string details)
        {
            if (details != null)
            {
                if (targetType == "DeepSky.OpenCluster")
                {
                    return JsonConvert.DeserializeObject<OpenClusterObservationDetails>(details);
                }
                else if (targetType.StartsWith("DeepSky"))
                {
                    return JsonConvert.DeserializeObject<DeepSkyObservationDetails>(details);
                }
            }
            return null;
        }

        private object DeserializeTargetDetails(string targetType, string details)
        {
            if (details != null)
            {
                if (targetType == "DeepSky.OpenCluster")
                {
                    return JsonConvert.DeserializeObject<DeepSkyOpenClusterTargetDetails>(details);
                }
                //else if (targetType.StartsWith("DeepSky"))
                //{
                //    return JsonConvert.DeserializeObject<DeepSkyTargetDetails>(details);
                //}
            }
            return null;
        }
    }
}
