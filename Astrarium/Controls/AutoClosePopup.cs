using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace Astrarium.Controls
{
    public class AutoClosePopup : Popup
    {
        private int ms = 1000;

        public async void Show()
        {
            if (!IsOpen)
            {
                IsOpen = true;
                await Task.Delay(ms);
                Application.Current.Dispatcher.Invoke(() => IsOpen = false);
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Window window = Window.GetWindow(this);
            window.LocationChanged += Window_LocationChanged;
            window.SizeChanged += Window_SizeChanged;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawPopup();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            RedrawPopup();
        }

        private void RedrawPopup()
        {
            HorizontalOffset += 1;
            HorizontalOffset -= 1;
        }

    }
}
