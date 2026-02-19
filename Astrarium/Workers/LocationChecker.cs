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
        private readonly ILocationDetector detector;
        private readonly ISettings settings = null;
       
        public LocationChecker(ILocationDetector detector, ISettings settings)
        {
            this.detector = detector;
            this.settings = settings;

            this.detector.OnLocationDetected += CheckLocation;
        }

        public async void Run()
        {
            if (settings.Get("CheckLocationOnStart") && Environment.OSVersion.Version.Major >= 10)
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
                detector.Detect();
            }
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
                        ViewManager.RaiseMessage("SelectLocation", actualLocation);
                    }
                });
            }
        }
    }
}
