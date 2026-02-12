using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Device.Location;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace Astrarium.Workers
{
    public class LocationChecker : IWorker
    {
        private readonly GeoCoordinateWatcher watcher = null;
        private readonly ISettings settings = null;

        public LocationChecker(ISettings settings)
        {
            this.settings = settings;
            watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);
            watcher.StatusChanged += StatusChanged;
        }

        public void Run()
        {
            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
                bool result = watcher.TryStart(true, TimeSpan.FromSeconds(10));
                if (result)
                {
                    if (watcher.Permission == GeoPositionPermission.Denied)
                    {
                        RequestPermission();
                    }
                }
            });
        }

        private void StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            if (e.Status == GeoPositionStatus.Ready)
            {
                if (watcher.Position.Location.IsUnknown)
                {
                    RequestPermission();
                }
                else
                {
                    TryGetPosition();
                }
            }
            else if (e.Status == GeoPositionStatus.Disabled ||
                    (e.Status == GeoPositionStatus.NoData &&
                    watcher.Permission == GeoPositionPermission.Denied))
            {
                RequestPermission();
            }

            //watcher.StatusChanged -= StatusChanged;
            //watcher.Stop();
        }

        private void TryGetPosition()
        {
            var currentLocation = watcher.Position.Location;
            if (!currentLocation.IsUnknown)
            {
                double utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalHours;
                var location = new CrdsGeographical(
                    -currentLocation.Longitude,
                    currentLocation.Latitude,
                    utcOffset, 0, Text.Get("MyCurrentLocation"));

                CheckLocation(location);
            }        
        }

        private void RequestPermission()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // ask user to enable location detection in settings
                if (ViewManager.ShowMessageBox("Attention", "To calculate more accurately positions of celestial bodies on the star map and make your observations more productive, the app requires access to your location. Would you like to open system settings and enable location services?", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
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
                    if (ViewManager.ShowMessageBox("Attention", $"It seems that your actual location is too far ({(int)distance} km away) from observer location specified in application settings. Would you like to change settings?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        ViewManager.RaiseCommand("SelectLocation", actualLocation);
                    }
                });
            }
        }

        private async void OpenSystemLocationSettings()
        {
            try
            {
                // Windows 10/11
                if (Environment.OSVersion.Version.Major >= 10)
                {
                    Process.Start("ms-settings:privacy-location");
                }
                // Previous Windows versions
                else
                {
                    Process.Start("control.exe", "input.dll,,{LOCATION}");
                }

                await Task.Delay(TimeSpan.FromMinutes(1));
                if (watcher.TryStart(true, TimeSpan.FromSeconds(10)))
                {
                    //TryGetPosition();
                }

            }
            catch (Exception ex)
            {
                Log.Error("Unable to open system location settings");
            }
        }
    }
}
