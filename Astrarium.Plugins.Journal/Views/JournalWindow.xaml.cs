using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Astrarium.Plugins.Journal.Views
{
    /// <summary>
    /// Interaction logic for JournalWindow.xaml
    /// </summary>
    public partial class JournalWindow : Window
    {
        public JournalWindow()
        {
            InitializeComponent();
        }

        private async void calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            await Task.Delay(100);
            calendar.DisplayDate = calendar.SelectedDate.Value;
        }

        private void CalendarDayButton_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var button = sender as CalendarDayButton;
            calendar.SelectedDate = (DateTime)button.DataContext;
        }

        private void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var menu = sender as ContextMenu;
            menu.DataContext = DataContext;
            menu.Placement = PlacementMode.Mouse;
        }

        private void calendar_GotMouseCapture(object sender, MouseEventArgs e)
        {
            UIElement originalElement = e.OriginalSource as UIElement;
            if (originalElement is CalendarDayButton || originalElement is CalendarItem)
            {
                originalElement.ReleaseMouseCapture();
            }
        }
    }
}
