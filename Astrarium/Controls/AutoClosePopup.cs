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
        private const int DELAY = 1000;
        
        private volatile int LastShowTime; 
        private bool IsClosingScheduled; 

        public void Show()
        {
            LastShowTime = Environment.TickCount;

            if (!IsOpen)
            {
                IsOpen = true;
                if (!IsClosingScheduled)
                {
                    IsClosingScheduled = true;
                    _ = RunAutoCloseCheckAsync();
                }
            }
        }

        private async Task RunAutoCloseCheckAsync()
        {
            while (true)
            {
                int timeSinceLastCall = Environment.TickCount - LastShowTime;
                if (timeSinceLastCall >= DELAY)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        IsOpen = false;
                        IsClosingScheduled = false;
                    });
                    break;
                }
                
                await Task.Delay(Math.Min(50, DELAY - timeSinceLastCall));
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
