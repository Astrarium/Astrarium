using Planetarium.Objects;
using Planetarium.Themes;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.ViewModels
{
    public class PhenomenaSettingsVM : ViewModelBase
    {
        private readonly Sky sky;
        private readonly IViewManager viewManager;

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
                        .Select(n => n.Text);
            }
        }

        public bool OkButtonEnabled
        {
            get
            {
                return Nodes.Any() && Nodes.First().IsChecked != false;
            }
        }

        public PhenomenaSettingsVM(Sky sky, IViewManager viewManager)
        {
            this.sky = sky;
            this.viewManager = viewManager;

            UtcOffset = sky.Context.GeoLocation.UtcOffset;

            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);

            BuildCategoriesTree();
        }

        public void Ok()
        {
            if (JulianDayFrom > JulianDayTo)
            {
                viewManager.ShowMessageBox("Warning", "Wrong date range:\nend date should be greater than start date.", System.Windows.MessageBoxButton.OK);
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
                Node node = new Node() { Text = group.Key };

                if (group.Count() > 1)
                {
                    foreach (var item in group)
                    {
                        node.Children.Add(new Node(item, item));
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
