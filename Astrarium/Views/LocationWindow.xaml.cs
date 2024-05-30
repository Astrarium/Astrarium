using Astrarium.Types.Controls;
using System;
using System.Windows;
using System.Windows.Forms.Integration;

namespace Astrarium.Views
{
    /// <summary>
    /// Interaction logic for LocationWindow.xaml
    /// </summary>
    public partial class LocationWindow : MouseEventsInterceptableWindow
    {
        public LocationWindow()
        {
            InitializeComponent();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (Map.ContextMenu.DataContext == null)
            {
                Map.ContextMenu.DataContext = DataContext;
            }
        }

        protected override WindowsFormsHost Host => Map;
    }
}
