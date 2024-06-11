using Astrarium.Types.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;

namespace Astrarium.Plugins.Eclipses
{
    /// <summary>
    /// Interaction logic for LunarEclipseWindow.xaml
    /// </summary>
    public partial class LunarEclipseWindow : MouseEventsInterceptableWindow
    {
        protected override WindowsFormsHost Host => Map;

        public LunarEclipseWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Map.Dispose();
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
