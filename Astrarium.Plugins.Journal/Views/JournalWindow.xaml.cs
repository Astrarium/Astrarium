using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null)
            {
                item.BringIntoView();
                e.Handled = true;
            }
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
        }
    }
}
