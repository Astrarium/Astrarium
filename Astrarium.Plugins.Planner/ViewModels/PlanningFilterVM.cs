using Astrarium.Algorithms;
using Astrarium.Types;
using Astrarium.Types.Themes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Astrarium.Plugins.Planner.ViewModels
{
    public class PlanningFilterVM : ViewModelBase
    {
        private ISky sky;

        public ICommand OkCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public PlanningFilter Filter
        {
            get
            {
                return new PlanningFilter()
                {
                    JulianDayMidnight = JulianDay,
                    TimeFrom = TimeFrom.TotalHours,
                    TimeTo = TimeTo.TotalHours,
                    ApplyFilters = ApplyFilters,
                    MagLimit = EnableMagLimit ? (float?)MagLimit : null,
                    MinBodyAltitude = EnableMinBodyAltitude ? (double?)MinBodyAltitude : null,
                    MaxSunAltitude = EnableMaxSunAltitude ? (double?)MaxSunAltitude : null,
                    CountLimit = EnableCountLimit ? (int?)CountLimit : null,
                    DurationLimit = EnableDurationLimit ? (double?)DurationLimit : null,
                    SkipUnknownMagnitude = SkipUnknownMagnitude,
                    CelestialObjects = CelestialObjects.ToArray(),
                    CelestialObjectsTypes = CelestialObjectsTypes.ToArray(),
                    ObserverLocation = sky.Context.GeoLocation
                };
            }
            set
            {

            }
        }

        /// <summary>
        /// Filter of celestial objects types.
        /// </summary>
        public ObservableCollection<Node> Nodes { get; private set; } = new ObservableCollection<Node>();

        /// <summary>
        /// Collection of celestial objects to be included in the plan. If set, types filter (<see cref="Nodes"/> is not taken into account.
        /// </summary>
        public ObservableCollection<CheckedListItem<CelestialObject>> ListItems { get; private set; } = new ObservableCollection<CheckedListItem<CelestialObject>>();

        public ICollection<CelestialObject> CelestialObjects
        {
            get => ListItems.Where(x => x.IsChecked).Select(x => x.Item).ToArray();
            set
            {
                if (value != null)
                {
                    ListItems = new ObservableCollection<CheckedListItem<CelestialObject>>(value.Select(x => new CheckedListItem<CelestialObject>(x, isChecked: true, c => NotifyPropertyChanged(nameof(OkButtonEnabled)))));
                }
                else
                {
                    ListItems = new ObservableCollection<CheckedListItem<CelestialObject>>();
                }

                NotifyPropertyChanged(
                    nameof(CelestialObjects),
                    nameof(ListItems),
                    nameof(IsCelestialObjectsListVisible));
            }
        }

        public ICollection<string> CelestialObjectsTypes
        {
            get => Nodes.Any() ? Nodes.First().CheckedChildIds : new string[0];
            set
            {
                Nodes.Clear();
                if (value != null)
                {
                    var groups = value.GroupBy(t => t.Split('.').First());

                    Node root = new Node("All");
                    root.CheckedChanged += Root_CheckedChanged;

                    foreach (var group in groups)
                    {
                        Node node = new Node(Text.Get($"{group.Key}.Type"), group.Key);
                        foreach (var item in group)
                        {
                            if (item != group.Key)
                            {
                                node.Children.Add(new Node(Text.Get($"{item}.Type"), item));
                            }
                        }
                        root.Children.Add(node);
                    }

                    Nodes.Add(root);
                }

                NotifyPropertyChanged(
                    nameof(CelestialObjectsTypes),
                    nameof(Nodes), 
                    nameof(IsCelestialObjectsTypesTreeVisible));
            }
        }

        public bool IsCelestialObjectsTypesTreeVisible => Nodes.Any();
        
        public bool IsCelestialObjectsListVisible => ListItems.Any();

        public double JulianDay
        {
            get => GetValue<double>(nameof(JulianDay));
            set => SetValue(nameof(JulianDay), value);
        }

        public double UtcOffset
        {
            get => GetValue<double>(nameof(UtcOffset));
            set => SetValue(nameof(UtcOffset), value);
        }

        public TimeSpan TimeFrom
        {
            get => GetValue<TimeSpan>(nameof(TimeFrom), new TimeSpan(22, 0, 0));
            set => SetValue(nameof(TimeFrom), value);
        }

        public TimeSpan TimeTo
        {
            get => GetValue<TimeSpan>(nameof(TimeTo), new TimeSpan(0, 0, 0));
            set => SetValue(nameof(TimeTo), value);
        }

        public bool ApplyFilters
        {
            get => GetValue<bool>(nameof(ApplyFilters), true);
            set => SetValue(nameof(ApplyFilters), value);
        }

        public bool EnableMagLimit
        {
            get => GetValue<bool>(nameof(EnableMagLimit));
            set => SetValue(nameof(EnableMagLimit), value);
        }

        public decimal MagLimit
        {
            get => GetValue<decimal>(nameof(MagLimit), 10);
            set => SetValue(nameof(MagLimit), value);
        }

        public bool EnableMinBodyAltitude
        {
            get => GetValue<bool>(nameof(EnableMinBodyAltitude), true);
            set => SetValue(nameof(EnableMinBodyAltitude), value);
        }

        public decimal MinBodyAltitude
        {
            get => GetValue<decimal>(nameof(MinBodyAltitude), 5);
            set => SetValue(nameof(MinBodyAltitude), value);
        }

        public bool EnableMaxSunAltitude
        {
            get => GetValue<bool>(nameof(EnableMaxSunAltitude));
            set => SetValue(nameof(EnableMaxSunAltitude), value);
        }

        public decimal MaxSunAltitude
        {
            get => GetValue<decimal>(nameof(MaxSunAltitude), 0);
            set => SetValue(nameof(MaxSunAltitude), value);
        }

        public bool EnableCountLimit
        {
            get => GetValue<bool>(nameof(EnableCountLimit), true);
            set => SetValue(nameof(EnableCountLimit), value);
        }

        public decimal CountLimit
        {
            get => GetValue<decimal>(nameof(CountLimit), 1000);
            set => SetValue(nameof(CountLimit), value);
        }

        public bool EnableDurationLimit
        {
            get => GetValue<bool>(nameof(EnableDurationLimit), true);
            set => SetValue(nameof(EnableDurationLimit), value);
        }

        public decimal DurationLimit
        {
            get => GetValue<decimal>(nameof(DurationLimit), 10);
            set => SetValue(nameof(DurationLimit), value);
        }

        public bool SkipUnknownMagnitude
        {
            get => GetValue<bool>(nameof(SkipUnknownMagnitude));
            set => SetValue(nameof(SkipUnknownMagnitude), value);
        }

        public bool OkButtonEnabled => CelestialObjectsTypes.Any() || CelestialObjects.Any();

        public PlanningFilterVM(ISky sky)
        {
            this.sky = sky;

            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);

            JulianDay = sky.Context.JulianDayMidnight;
            UtcOffset = sky.Context.GeoLocation.UtcOffset;
        }

        private void Root_CheckedChanged(object sender, bool? e)
        {
            NotifyPropertyChanged(nameof(OkButtonEnabled));
        }

        private void Ok()
        {
            Close(true);
        }

        public class CheckedListItem<T> : PropertyChangedBase
        {
            private bool _IsChecked;
            public bool IsChecked
            {
                get => _IsChecked;
                set
                {
                    _IsChecked = value;
                    NotifyPropertyChanged(nameof(IsChecked));
                    if (OnCheckedChanged != null)
                        OnCheckedChanged.Invoke(_IsChecked);
                }
            }

            public T Item { get; set; }

            private Action<bool> OnCheckedChanged;

            public CheckedListItem(T item, bool isChecked = true, Action<bool> onCheckedChanged = null)
            {
                Item = item;
                IsChecked = isChecked;
                OnCheckedChanged = onCheckedChanged;
            }
        }
    }
}
