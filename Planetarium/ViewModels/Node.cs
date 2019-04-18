using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.ViewModels
{
    public class Node : INotifyPropertyChanged
    {
        public Node()
        {
            Children.CollectionChanged += Children_CollectionChanged;
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

        private string text;
        private bool? isChecked = true;
        private bool isExpanded = true;

        public ObservableCollection<Node> Children { get; } = new ObservableCollection<Node>();

        public event EventHandler<bool?> CheckedChanged;

        public bool? IsChecked
        {
            get { return isChecked; }
            set
            {
                if (value != isChecked)
                {
                    isChecked = value;
                    RaisePropertyChanged(nameof(IsChecked));
                    CheckedChanged?.Invoke(this, isChecked);
                    if (value != null)
                    {
                        AllChildren(this).ToList().ForEach(c => c.IsChecked = value);
                    }
                }
            }
        }

        public string Text
        {
            get { return text; }
            set
            {
                if (value != text)
                {
                    text = value;
                    RaisePropertyChanged(nameof(Text));
                }
            }
        }

        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if (value != isExpanded)
                {
                    isExpanded = value;
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
