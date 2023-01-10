using System.Windows;

namespace Astrarium.Views
{
    /// <summary>
    /// Interaction logic for LocationWindow.xaml
    /// </summary>
    public partial class LocationWindow : Window
    {
        public LocationWindow()
        {
            InitializeComponent();
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (Map.ContextMenu.DataContext == null)
            {
                Map.ContextMenu.DataContext = DataContext;
            }
        }
    }
}
