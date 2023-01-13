using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Data;

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
            PropertyChangedCallback = new PropertyChangedCallback(SelectedTreeViewItemPropertyChanged)
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

        private static void SelectedTreeViewItemPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var treeView = sender as BindableTreeView;
            var collectionView = treeView.ItemsSource as CollectionView;

            if (collectionView != null)
            {
                var value = e.NewValue;
                int index = collectionView.IndexOf(value);
                if (index >= 0)
                {
                    VirtualizingStackPanel itemHost = FindVisualChild<VirtualizingStackPanel>(treeView);
                    if (itemHost != null)
                    {
                        itemHost.BringIndexIntoViewPublic(index);
                        ItemContainerGenerator gen = treeView.ItemContainerGenerator;
                        TreeViewItem tvi = gen.ContainerFromItem(value) as TreeViewItem;
                        tvi.IsSelected = true;
                    }
                }
            }
        }

        private static T FindVisualChild<T>(Visual visual) where T : Visual
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
            {
                Visual child = (Visual)VisualTreeHelper.GetChild(visual, i);
                if (child != null)
                {
                    T correctlyTyped = child as T;
                    if (correctlyTyped != null)
                    {
                        return correctlyTyped;
                    }

                    T descendent = FindVisualChild<T>(child);
                    if (descendent != null)
                    {
                        return descendent;
                    }
                }
            }

            return null;
        }
    }
}
