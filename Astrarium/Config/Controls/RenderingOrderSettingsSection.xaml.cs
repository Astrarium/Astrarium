using Astrarium.Types.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Astrarium.Config.Controls
{
    /// <summary>
    /// Interaction logic for RenderingOrderSettingsSection.xaml
    /// </summary>
    public partial class RenderingOrderSettingsSection : SettingsSection
    {
        public RenderingOrderSettingsSection()
        {
            InitializeComponent();

            Style itemContainerStyle = new Style(typeof(ListBoxItem), (Style)Application.Current.TryFindResource(typeof(ListBoxItem)));
            itemContainerStyle.Setters.Add(new Setter(AllowDropProperty, true));
            itemContainerStyle.Setters.Add(new EventSetter(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(PreviewMouseLeftButtonDownHandler)));
            itemContainerStyle.Setters.Add(new EventSetter(DropEvent, new DragEventHandler(DropHandler)));
            List.ItemContainerStyle = itemContainerStyle;
        }

        private void PreviewMouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem)
            {
                ListBoxItem draggedItem = sender as ListBoxItem;
                DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
                draggedItem.IsSelected = true;
            }
        }

        private void DropHandler(object sender, DragEventArgs e)
        {
            var renderingOrder = List.ItemsSource as RenderingOrder;

            var fromItem = e.Data.GetData(typeof(RenderingOrderItem)) as RenderingOrderItem;
            var toItem = ((ListBoxItem)sender).DataContext as RenderingOrderItem;

            int oldIndex = List.Items.IndexOf(fromItem);
            int newIndex = List.Items.IndexOf(toItem);

            renderingOrder.Move(oldIndex, newIndex);
        }

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
