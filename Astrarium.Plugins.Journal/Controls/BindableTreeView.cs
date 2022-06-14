using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Astrarium.Plugins.Journal.Controls
{
    public class BindableTreeView : TreeView
    {
        public BindableTreeView() : base()
        {
            SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(HandleSelectedItemChanged);
        }

        void HandleSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (SelectedItem != null)
            {
                SetValue(SelectedTreeViewItemProperty, SelectedItem);
            }
        }

        public object SelectedTreeViewItem
        {
            get { return GetValue(SelectedTreeViewItemProperty); }
            set { SetValue(SelectedTreeViewItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedTreeViewItemProperty = DependencyProperty.Register(nameof(SelectedTreeViewItem), typeof(object), typeof(BindableTreeView), new UIPropertyMetadata(null));
    }
}
