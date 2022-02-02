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

        public PlanningFilterVM(ISky sky)
        {
            this.sky = sky;

            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);

            JulianDay = sky.Context.JulianDayMidnight;
            UtcOffset = sky.Context.GeoLocation.UtcOffset;

            BuildCategoriesTree();
        }

        private void BuildCategoriesTree()
        {
            var types = sky.CelestialObjects.Select(c => c.Type).Where(t => t != null).Distinct();
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

        public bool OkButtonEnabled => ObjectTypes.Any() && ObjectTypes.First().IsChecked != false;

        private void Root_CheckedChanged(object sender, bool? e)
        {
            NotifyPropertyChanged(nameof(OkButtonEnabled));
        }

        private IEnumerable<Node> AllNodes(Node node)
        {
            yield return node;

            foreach (Node child in node.Children)
            {
                foreach (Node n in AllNodes(child))
                {
                    yield return n;
                }
            }
        }

        private string[] GetObjectTypes()
        {
            return AllNodes(ObjectTypes.First())
                    .Where(n => n.IsChecked ?? false)
                    .Select(n => n.Id).ToArray();
        }

        private void Ok()
        {
            Filter = new PlanningFilter()
            {
                JulianDayMidnight = JulianDay,
                MagLimit = EnableMagLimit ? (float?)MagLimit : null,
                TimeFrom = TimeFrom.TotalHours,
                TimeTo = 3,
                MinBodyAltitude = 0,
                MinSunAltitude = 0,
                ObjectTypes = GetObjectTypes(),
                ObserverLocation = sky.Context.GeoLocation
            };

            Close(true);
        }
    }
}
