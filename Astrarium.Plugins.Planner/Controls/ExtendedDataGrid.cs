using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Astrarium.Plugins.Planner.Controls
{
    public class ExtendedDataGrid : DataGrid
    {
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(nameof(SelectedItems), typeof(IList), typeof(ExtendedDataGrid), new PropertyMetadata(default(IList), OnSelectedItemsPropertyChanged));

        //static ExtendedDataGrid()
        //{
        //    DefaultStyleKeyProperty.
        //    DefaultStyleKeyProperty.OverrideMetadata(typeof(ExtendedDataGrid), new FrameworkPropertyMetadata(typeof(DataGrid)));
        //}

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            SetValue(SelectedItemsProperty, base.SelectedItems);
        }

        public new IList SelectedItems
        {
            get => (IList)GetValue(SelectedItemsProperty);
            set => throw new Exception("This property is read-only. To bind to it you must use 'Mode=OneWayToSource'.");
        }

        private static void OnSelectedItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ExtendedDataGrid)d).OnSelectedItemsChanged((IList)e.OldValue, (IList)e.NewValue);
        }

        protected virtual void OnSelectedItemsChanged(IList oldSelectedItems, IList newSelectedItems) { }
    }
}
