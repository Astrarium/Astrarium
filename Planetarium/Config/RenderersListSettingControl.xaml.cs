using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Planetarium.Config
{
    /// <summary>
    /// Interaction logic for RenderersListSettingControl.xaml
    /// </summary>
    public partial class RenderersListSettingControl : UserControl
    {
        public RenderersListSettingControl()
        {
            InitializeComponent();

            //Style itemContainerStyle = new Style(typeof(ListBoxItem));
            //itemContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
            //itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(s_PreviewMouseLeftButtonDown)));
            //itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent, new DragEventHandler(listbox1_Drop)));
            //List.ItemContainerStyle = itemContainerStyle;
        }

        //void s_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{

        //    if (sender is ListBoxItem)
        //    {
        //        ListBoxItem draggedItem = sender as ListBoxItem;
        //        DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
        //        draggedItem.IsSelected = true;
        //    }
        //}

        //void listbox1_Drop(object sender, DragEventArgs e)
        //{
        //    RenderingOrderItem droppedData = e.Data.GetData(typeof(RenderingOrderItem)) as RenderingOrderItem;
        //    RenderingOrderItem target = ((ListBoxItem)(sender)).DataContext as RenderingOrderItem;

        //    int removedIdx = List.Items.IndexOf(droppedData);
        //    int targetIdx = List.Items.IndexOf(target);

        //    var renderingOrder = List.ItemsSource as RenderingOrder;
        //    renderingOrder.Move(removedIdx, targetIdx);
        //}

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            var renderingOrder = List.ItemsSource as RenderingOrder;
            int index = List.SelectedIndex;
            if (index > 0)
            {
                renderingOrder.Move(index, index - 1);
            }
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            var renderingOrder = List.ItemsSource as RenderingOrder;
            int index = List.SelectedIndex;
            if (index < List.Items.Count - 1)
            {
                renderingOrder.Move(index, index + 1);
            }
        }
    }
}
