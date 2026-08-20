using Astrarium.Types.Controls;
using System.Windows.Controls;
using System.Windows.Input;

namespace Astrarium.Plugins.SolarSystem.Controls
{
    /// <summary>
    /// Interaction logic for SolarActivityControl.xaml
    /// </summary>
    public partial class SolarActivityControl : DisposableUserControl
    {
        public SolarActivityControl()
        {
            InitializeComponent();
        }

        private void BindableListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // solution from: https://stackoverflow.com/questions/61870147/wpf-datagrid-inside-a-scrollviewer
            var args = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            args.RoutedEvent = ScrollViewer.MouseWheelEvent;
            scroll_viewer.RaiseEvent(args);
        }
    }
}
