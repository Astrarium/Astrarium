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

        public PlanningFilter Filter { get; private set; }

        public ObservableCollection<Node> ObjectTypes { get; private set; } = new ObservableCollection<Node>();

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
            get => GetValue<bool>(nameof(EnableMinBodyAltitude));
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
            get => GetValue<bool>(nameof(EnableCountLimit));
            set => SetValue(nameof(EnableCountLimit), value);
        }

        public decimal CountLimit
        {
            get => GetValue<decimal>(nameof(CountLimit), 1000);
            set => SetValue(nameof(CountLimit), value);
        }

        public bool EnableDurationLimit
        {
            get => GetValue<bool>(nameof(EnableDurationLimit));
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


        public bool OkButtonEnabled => ObjectTypes.Any() && ObjectTypes.First().IsChecked != false;

        public PlanningFilterVM(ISky sky)
        {
            this.sky = sky;

            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);

            JulianDay = sky.Context.JulianDayMidnight;
            UtcOffset = sky.Context.GeoLocation.UtcOffset;

            IEnumerable<string> types = sky.CelestialObjects.Select(c => c.Type).Where(t => t != null).Distinct();
            var groups = types.GroupBy(t => t.Split('.').First());

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

            ObjectTypes.Add(root);
        }

        private void Root_CheckedChanged(object sender, bool? e)
        {
            NotifyPropertyChanged(nameof(OkButtonEnabled));
        }

        private void Ok()
        {
            Filter = new PlanningFilter()
            {
                JulianDayMidnight = JulianDay,
                MagLimit = EnableMagLimit ? (float?)MagLimit : null,
                TimeFrom = TimeFrom.TotalHours,
                TimeTo = TimeTo.TotalHours,
                MinBodyAltitude = EnableMinBodyAltitude ? (double?)MinBodyAltitude : null,
                MaxSunAltitude = EnableMaxSunAltitude ? (double?)MaxSunAltitude : null,
                CountLimit = EnableCountLimit ? (int?)CountLimit : null,
                DurationLimit = EnableDurationLimit ? (double?)DurationLimit : null,
                SkipUnknownMagnitude = SkipUnknownMagnitude,
                ObjectTypes = ObjectTypes.First().CheckedChildIds,
                ObserverLocation = sky.Context.GeoLocation
            };

            Close(true);
        }
    }
}
