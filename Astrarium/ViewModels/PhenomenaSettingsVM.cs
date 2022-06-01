using Astrarium.Types.Themes;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (JulianDayFrom > JulianDayTo)
            {
                ViewManager.ShowMessageBox("$PhenomenaSettingsWindow.WarningTitle", "$PhenomenaSettingsWindow.WarningText", System.Windows.MessageBoxButton.OK);
                return;
            }

            Close(true);
        }

        private void BuildCategoriesTree()
        {
            Nodes.Clear();

            var categories = sky.GetEventsCategories();
            var groups = categories.GroupBy(cat => cat.Split('.').First());

            Node root = new Node(Text.Get("PhenomenaSettingsWindow.Phenomena.All"));
            root.CheckedChanged += Root_CheckedChanged;

            foreach (var group in groups)
            {
                Node node = new Node(Text.Get(group.Key), group.Key);
                foreach (var item in group)
                {
                    if (item != group.Key)
                    {
                        node.Children.Add(new Node(Text.Get(item), item));
                    }
                }
                root.Children.Add(node);
            }

            Nodes.Add(root);
            root.IsChecked = true;
        }

        private void Root_CheckedChanged(object sender, bool? e)
        {
            NotifyPropertyChanged(nameof(OkButtonEnabled));
        }
    }
}
