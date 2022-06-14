using Astrarium.Algorithms;
using Astrarium.Plugins.Journal.Database;
using Astrarium.Plugins.Journal.Database.Entities;
using Astrarium.Types;
using Astrarium.Types.Themes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.ViewModels
{
    public class TreeItemSession : PropertyChangedBase
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

        public string Comments
        {
            get => GetValue<string>(nameof(Comments), null);
            set => SetValue(nameof(Comments), value);
        }

        public string Weather
        {
            get => GetValue<string>(nameof(Weather), null);
            set => SetValue(nameof(Weather), value);
        }

        public int? Seeing
        {
            get => GetValue<int?>(nameof(Seeing), null);
            set => SetValue(nameof(Seeing), value);
        }

        public decimal? FaintestStar
        {
            get => GetValue<decimal?>(nameof(FaintestStar), null);
            set => SetValue(nameof(FaintestStar), value);
        }

        public decimal? SkyQuality
        {
            get => GetValue<decimal?>(nameof(SkyQuality), null);
            set => SetValue(nameof(SkyQuality), value);
        }

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

    public class TreeItemObservation : PropertyChangedBase
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

        public string Findings
        {
            get => GetValue<string>(nameof(Findings));
            set => SetValue(nameof(Findings), value);
        }

        public object Details
        {
            get => GetValue<object>(nameof(Details));
            set => SetValue(nameof(Details), value);
        }

        public object TargetDetails
        {
            get => GetValue<object>(nameof(TargetDetails));
            set => SetValue(nameof(TargetDetails), value);
        }

        public string Telescope
        {
            get => GetValue<string>(nameof(Telescope), null);
            set => SetValue(nameof(Telescope), value);
        }

        public string Eyepiece
        {
            get => GetValue<string>(nameof(Eyepiece), null);
            set => SetValue(nameof(Eyepiece), value);
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

        public string Lens
        {
            get => GetValue<string>(nameof(Lens), null);
            set => SetValue(nameof(Lens), value);
        }
    }

    public class JournalVM : ViewModelBase
    {
        public ObservableCollection<TreeItemSession> AllSessions { get; } = new ObservableCollection<TreeItemSession>();

        public object SelectedTreeViewItem
        {
            get => GetValue<object>(nameof(SelectedTreeViewItem));
            set
            {
                SetValue(nameof(SelectedTreeViewItem), value);
                LoadSelectedTreeViewItemDetails();
            }
        }

        public JournalVM()
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
            }
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
                    session.FaintestStar = (decimal?)s.FaintestStar;
                    session.SkyQuality = (decimal?)s.SkyQuality;
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
                        .Include(x => x.Scope)
                        .Include(x => x.Eyepiece)
                        .Include(x => x.Lens)
                        .FirstOrDefault(x => x.Id == observation.Id);

                    observation.Findings = obs.Result;
                    observation.Details = DeserializeObservationDetails(obs.Target.Type, obs.Details);
                    observation.TargetDetails = DeserializeTargetDetails(obs.Target.Type, obs.Target.Details);

                    observation.Telescope = obs.Scope != null ? (obs.Scope.Vendor + " " + obs.Scope.Model) : null;
                    observation.Eyepiece = obs.Eyepiece != null ? (obs.Eyepiece.Vendor + " " + obs.Eyepiece.Model) : null;
                    observation.Lens = obs.Lens != null ? (obs.Lens?.Vendor + " " + obs.Lens.Model) : null;

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

        private object DeserializeObservationDetails(string targetType, string details)
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
