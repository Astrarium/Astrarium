using Astrarium.Objects;
using Astrarium.Types.Themes;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Astrarium.Types.Localization;

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

        public IEnumerable<string> Categories
        {
            get
            {
                return
                    AllNodes(Nodes.First())
                        .Where(n => n.IsChecked ?? false)
                        .Select(n => n.Id);
            }
        }

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

            UtcOffset = sky.Context.GeoLocation.UtcOffset;

            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);

            BuildCategoriesTree();
        }

        public void Ok()
        {
            if (JulianDayFrom > JulianDayTo)
            {
                ViewManager.ShowMessageBox("Warning", "Wrong date range:\nend date should be greater than start date.", System.Windows.MessageBoxButton.OK);
                return;
            }

            // everything is fine
            Close(true);
        }

        private void BuildCategoriesTree()
        {
            Nodes.Clear();

            var categories = sky.GetEventsCategories();

            var groups = categories.GroupBy(cat => cat.Split('.').First());

            Node root = new Node("All");
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

        private void Root_CheckedChanged(object sender, bool? e)
        {
            NotifyPropertyChanged(nameof(OkButtonEnabled));
        }
    }
}
