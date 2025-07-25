using Astrarium.Types.Themes;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Astrarium.Algorithms;

namespace Astrarium.ViewModels
{
    public class PhenomenaSettingsVM : ViewModelBase
    {
        private readonly ISky sky;

        public ObservableCollection<Node> Nodes { get; private set; } = new ObservableCollection<Node>();
        public Command OkCommand { get; private set; }
        public Command CancelCommand { get; private set; }

        public double JulianDayFrom { get; set; }
        public double JulianDayTo { get; set; }
        public double UtcOffset { get; private set; }

        public IEnumerable<string> Categories => Nodes.First().CheckedChildIds;

        public bool OkButtonEnabled
        {
            get
            {
                return Nodes.Any() && Nodes.First().IsChecked != false;
            }
        }

        public PhenomenaSettingsVM(ISky sky)
        {
            this.sky = sky;

            JulianDayFrom = sky.Context.JulianDay;
            JulianDayTo = sky.Context.JulianDay + 30;

            UtcOffset = sky.Context.GeoLocation.UtcOffset;

            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);

            BuildCategoriesTree();
        }

        public void Ok()
        {
            if (new Date(JulianDayFrom).Year <= 0)
            {
                ViewManager.ShowMessageBox("$PhenomenaSettingsWindow.WarningTitle", "$PhenomenaSettingsWindow.NoPhenomenaForBCDates", System.Windows.MessageBoxButton.OK);
                return;
            }

            if (JulianDayFrom > JulianDayTo)
            {
                ViewManager.ShowMessageBox("$PhenomenaSettingsWindow.WarningTitle", "$PhenomenaSettingsWindow.WarningText", System.Windows.MessageBoxButton.OK);
                return;
            }

            Close(true);
        }

        public override object Payload => new { From = new Date( JulianDayFrom, UtcOffset).ToString(), To = new Date(JulianDayTo, UtcOffset).ToString(), Categories = string.Join("; ", Categories) };

        private void BuildCategoriesTree()
        {
            Nodes.Clear();

            var categories = sky.GetEventsCategories();
            var groups = categories.GroupBy(cat => cat.Split('.').First());

            Node root = new Node(Text.Get("PhenomenaSettingsWindow.Phenomena.All"));
            root.IsChecked = true;
            root.CheckedChanged += Root_CheckedChanged;
            Node daily = null;

            foreach (var group in groups)
            {
                Node node = new Node(Text.Get(group.Key), group.Key);
                foreach (var item in group)
                {
                    if (item != group.Key)
                    {
                        var child = new Node(Text.Get(item), item);
                        child.IsChecked = !item.StartsWith("Daily");
                        node.Children.Add(child);
                    }
                }

                // add regular (not daily) node to the root
                if (group.Key.StartsWith("Daily")) 
                {
                    node.IsChecked = false;
                    daily = node;
                }
                else
                {
                    node.IsChecked = true;
                    root.Children.Add(node);
                }
            }

            // add daily events node to the end
            if (daily != null)
            {
                root.Children.Add(daily);
            }

            Nodes.Add(root);
        }

        private void Root_CheckedChanged(object sender, bool? e)
        {
            NotifyPropertyChanged(nameof(OkButtonEnabled));
        }
    }
}
