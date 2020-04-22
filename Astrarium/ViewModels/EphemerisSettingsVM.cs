using Astrarium.Types.Themes;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Astrarium.ViewModels
{
    public class EphemerisSettingsVM : ViewModelBase
    {
        private readonly ISky sky;

        public ObservableCollection<Node> Nodes { get; private set; } = new ObservableCollection<Node>();
        public Command OkCommand { get; private set; }
        public Command CancelCommand { get; private set; }

        public double JulianDayFrom { get; set; }
        public double JulianDayTo { get; set; }
        public TimeSpan Step { get; set; } = TimeSpan.FromDays(1);
        public double UtcOffset { get; private set; }

        private CelestialObject _SelectedBody = null;
        public CelestialObject SelectedBody
        {
            get
            {
                return _SelectedBody;
            }
            set
            {
                _SelectedBody = value;
                BuildCategoriesTree();
                NotifyPropertyChanged(nameof(SelectedBody));
                NotifyPropertyChanged(nameof(OkButtonEnabled));
            }
        }

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

        public EphemerisSettingsVM(ISky sky)
        {
            this.sky = sky;

            UtcOffset = sky.Context.GeoLocation.UtcOffset;

            OkCommand = new Command(Ok);
            CancelCommand = new Command(Close);
        }

        public void Ok()
        {
            if (JulianDayFrom > JulianDayTo)
            {
                ViewManager.ShowMessageBox("Warning", "Wrong date range:\nend date should be greater than start date.", System.Windows.MessageBoxButton.OK);
                return;
            }

            if (Step < TimeSpan.FromSeconds(1))
            {
                ViewManager.ShowMessageBox("Warning", "Wrong step value:\nit's too small to calculate ephemerides.", System.Windows.MessageBoxButton.OK);
                return;
            }

            if ((JulianDayTo - JulianDayFrom) / Step.TotalDays > 10000)
            {
                ViewManager.ShowMessageBox("Warning", "Step value and date range mismatch:\nresulting ephemeris table is too large. Please increase the calculation step or reduce the date range.", System.Windows.MessageBoxButton.OK);
                return;
            }

            // everything is fine
            Close(true);
        }

        private void BuildCategoriesTree()
        {
            Nodes.Clear();

            if (SelectedBody != null)
            {
                var categories = sky.GetEphemerisCategories(SelectedBody);

                var groups = categories.GroupBy(cat => cat.Split('.').First());

                Node root = new Node("All");
                root.CheckedChanged += Root_CheckedChanged;

                string selectedBodyTypeName = SelectedBody.GetType().Name;

                foreach (var group in groups)
                {
                    Node node = new Node(Text.Get($"{selectedBodyTypeName}.{group.Key}"), group.Key);

                    if (group.Count() > 1)
                    {
                        foreach (var item in group)
                        {
                            node.Children.Add(new Node(Text.Get($"{selectedBodyTypeName}.{item}"), item));
                        }
                    }

                    root.Children.Add(node);
                }

                Nodes.Add(root);
            }
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
