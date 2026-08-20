using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Device.Location;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace Astrarium
{
    public interface ILocationDetector
    {
        event Action<CrdsGeographical> OnLocationDetected;
        void Detect();
    }

    public class LocationDetector : ILocationDetector
    {
        private GeoCoordinateWatcher watcher = null;
        private bool permissionRequested = false;

        public event Action<CrdsGeographical> OnLocationDetected;

        public void Detect()
        {
            Task.Run(async () =>
            {
                RestartWatcher();
            });
        }

        private void RestartWatcher()
        {
            if (watcher != null)
            {
                CloseWatcher();
            }
            watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);
            watcher.StatusChanged += StatusChanged;
            watcher.Start();
        }

        private void CloseWatcher()
        {
            watcher.StatusChanged -= StatusChanged;
            watcher.Dispose();
        }

        private async void StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            if (e.Status == GeoPositionStatus.Ready && !watcher.Position.Location.IsUnknown)
            {
                TryGetPosition();
            }
            else if (e.Status == GeoPositionStatus.NoData && watcher.Permission == GeoPositionPermission.Denied && watcher.Position.Location.IsUnknown)
            {
                if (!permissionRequested)
                {
                    RequestPermission();
                }
                else
                {
                    await Task.Delay(5000);
                    RestartWatcher();
                }
            }
        }

        private void RequestPermission()
        {
            permissionRequested = true;
            Application.Current.Dispatcher.Invoke(() =>
            {
                // ask user to enable location detection in settings
                if (ViewManager.ShowMessageBox("$Warning", "$EnableLocationServicesRequest", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    OpenSystemLocationSettings();
                }
            });
        }

        private void TryGetPosition()
        {
            var position = watcher.Position.Location;
            double utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalHours;
            var actualLocation = new CrdsGeographical(
                -position.Longitude,
                position.Latitude,
                utcOffset, 0, Text.Get("MyCurrentLocation"));

            CloseWatcher();

            Application.Current.Dispatcher.Invoke(() =>
            {
                OnLocationDetected?.Invoke(actualLocation);
            });
        }

        private void OpenSystemLocationSettings()
        {
            try
            {
                // Works on Windows 10 and above
                Process.Start("ms-settings:privacy-location");
                RestartWatcher();
            }
            catch (Exception ex)
            {
                Log.Error("Unable to open system location settings");
            }
        }
    }
}
