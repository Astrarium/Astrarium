using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Types.Themes
{
    public class Node : INotifyPropertyChanged
    {
        private Node()
        {
            Children.CollectionChanged += Children_CollectionChanged;
        }

        public Node(string text) : this()
        {
            Text = text;
        }

        public Node(string text, string id) : this()
        {
            Text = text;
            Id = id;
        }

        public string[] CheckedChildIds
        {
            get => AllChildren(this)
                  .Where(n => n.IsChecked ?? false)
                  .Select(n => n.Id).ToArray();
            set => AllChildren(this).Where(n => value.Contains(n.Id)).ToList().ForEach(n => n.IsChecked = true);
        }

        private IEnumerable<Node> AllChildren(Node node)
        {
            foreach (Node child in node.Children)
            {
                yield return child;
                foreach (Node n in AllChildren(child))
                {
                    yield return n;
                }
            }
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (var child in Children)
            {
                child.PropertyChanged += Child_PropertyChanged;
            }
        }

        private void Child_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsChecked))
            {
                if (AllChildren(this).All(c => c.IsChecked.HasValue && c.IsChecked.Value))
                {
                    IsChecked = true;
                }
                else if (AllChildren(this).All(c => c.IsChecked.HasValue && !c.IsChecked.Value))
                {
                    IsChecked = false;
                }
                else
                {
                    IsChecked = null;
                }
            }
        }

        public ObservableCollection<Node> Children { get; } = new ObservableCollection<Node>();

        public event EventHandler<bool?> CheckedChanged;

        private bool? _IsChecked = false;
        public bool? IsChecked
        {
            get { return _IsChecked; }
            set
            {
                if (value != _IsChecked)
                {
                    _IsChecked = value;
                    RaisePropertyChanged(nameof(IsChecked));
                    CheckedChanged?.Invoke(this, _IsChecked);
                    if (value != null)
                    {
                        AllChildren(this).ToList().ForEach(c => c.IsChecked = value);
                    }
                }
            }
        }

        public string Id { get; set; }

        private string _Text;
        public string Text
        {
            get { return _Text; }
            set
            {
                if (value != _Text)
                {
                    _Text = value;
                    RaisePropertyChanged(nameof(Text));
                }
            }
        }

        private bool _IsExpanded = true;
        public bool IsExpanded
        {
            get { return _IsExpanded; }
            set
            {
                if (value != _IsExpanded)
                {
                    _IsExpanded = value;
                    RaisePropertyChanged(nameof(IsExpanded));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
