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
using System.Windows.Shapes;

namespace Astrarium.Plugins.Eclipses
{
    /// <summary>
    /// Interaction logic for SolarEclipseWindow.xaml
    /// </summary>
    public partial class SolarEclipseWindow : Window
    {
        public SolarEclipseWindow()
        {
            InitializeComponent();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void RightPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustRightPanelControls();
        }

        private void RightPanel_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            AdjustRightPanelControls();
        }

        private void AdjustRightPanelControls()
        {
            double width = RightPanel.ActualWidth - (RightPanel.ComputedVerticalScrollBarVisibility == Visibility.Visible ? 17 : 0);
            RightPanelStack.Width = Math.Max(0, width);
            //RightPanelFooter.Width = width;
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
