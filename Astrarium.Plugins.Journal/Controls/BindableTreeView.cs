using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

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

        public static readonly DependencyProperty SelectedTreeViewItemProperty = DependencyProperty.Register(nameof(SelectedTreeViewItem), typeof(object), typeof(BindableTreeView), new FrameworkPropertyMetadata(null) { 
            DefaultUpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged,
            AffectsRender = true,
            BindsTwoWayByDefault = true,
            PropertyChangedCallback = new PropertyChangedCallback(DependencyPropertyChanged)
        });

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && source is Visual && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }

        private static void DependencyPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var treeView = sender as BindableTreeView;
            var tvi = treeView.ItemContainerGenerator.ContainerFromItem(e.NewValue) as TreeViewItem;
            if (tvi != null)
            {
                tvi.IsSelected = true;
            }
        }
    }
}
