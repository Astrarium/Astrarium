using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Astrarium.Types.Controls
{
    public class DisposableUserControl : UserControl
    {
        public DisposableUserControl()
        {
            Loaded += ControlLoaded;
        }

        private void ControlLoaded(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Closing += WindowClosing;
        }

        private void WindowClosing(object sender, EventArgs e)
        {
            (sender as Window).Closing -= WindowClosing;
            Loaded -= ControlLoaded;
            if (DataContext is IDisposable dc)
            {
                dc.Dispose();
            }
        }
    }
}
