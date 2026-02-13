using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Device.Location;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace Astrarium.Workers
{
    public class LocationChecker : IWorker
    {
        private readonly ISettings settings = null;
        private GeoCoordinateWatcher watcher = null;
        private bool permissionRequested = false;

        public LocationChecker(ISettings settings)
        {
            this.settings = settings;
        }

        public void Run()
        {
            if (settings.Get("CheckLocationOnStart") && Environment.OSVersion.Version.Major >= 10)
            {
                Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    RestartWatcher();
                });
            }
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
            watcher.Stop();
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

        private void TryGetPosition()
        {
            var position = watcher.Position.Location;
            double utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalHours;
            var actualLocation = new CrdsGeographical(
                -position.Longitude,
                position.Latitude,
                utcOffset, 0, Text.Get("MyCurrentLocation"));

            CloseWatcher();
            CheckLocation(actualLocation);
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

        private void CheckLocation(CrdsGeographical actualLocation)
        {
            // get location saved in settings
            var savedLocation = settings.Get<CrdsGeographical>("ObserverLocation");

            // measure distance from saved location and actual one
            var distance = savedLocation != null ? actualLocation.DistanceTo(savedLocation) : 0;

            // distance less than 50 km supposed to be OK
            if (distance > 50) 
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // ask user to change location
                    if (ViewManager.ShowMessageBox("$Warning", Text.Get("DetectedLocationIsFarAwayMessage", ("distance", ((int)distance).ToString())), MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        ViewManager.RaiseCommand("SelectLocation", actualLocation);
                    }
                });
            }
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
