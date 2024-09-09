using Astrarium.Algorithms;
using Astrarium.Types;
using Astrarium.Types.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium.Plugins.SolarSystem.ViewModels
{
    public class PlanetsSettingsVM : SettingsViewModel
    {
        private readonly PlanetsCalc calc;
        private readonly OrbitalElementsManager orbitalElementsManager;

        public Command UpdateGRSLongitudeCommand { get; private set; }
        public Command UpdateGenericMoonsOrbitalElementsCommand { get; private set; }

        public bool IsGRSSectionEnabled
        {
            get => GetValue<bool>(nameof(IsGRSSectionEnabled));
            set => SetValue(nameof(IsGRSSectionEnabled), value);
        }

        public bool OrbitalElementsIsReady
        {
            get => GetValue<bool>(nameof(OrbitalElementsIsReady));
            set => SetValue(nameof(OrbitalElementsIsReady), value);
        }

        public string GenericMoonsOrbitalElementsLastUpdated
        {
            get
            {
                var timestamp = Settings.Get<DateTime>("GenericMoonsOrbitalElementsLastUpdated");
                return timestamp < new DateTime(2000, 1, 1) ? Text.Get("GenericMoonsOrbitalElementsLastUpdated.Unknown") : Formatters.DateTime.Format(timestamp);
            }
        }

        public GreatRedSpotSettings GRSLongitude
        {
            get => Settings.Get<GreatRedSpotSettings>("GRSLongitude");
            set => Settings.Set("GRSLongitude", value);
        }

        public PlanetsSettingsVM(PlanetsCalc calc, OrbitalElementsManager orbitalElementsManager, ISettings settings) : base(settings)
        {
            this.calc = calc;
            this.orbitalElementsManager = orbitalElementsManager;
            UpdateGRSLongitudeCommand = new Command(UpdateGRSLongitude);
            UpdateGenericMoonsOrbitalElementsCommand = new Command(UpdateGenericMoonsOrbitalElements);
            OrbitalElementsIsReady = true;
            IsGRSSectionEnabled = true;
        }

        private async void UpdateGRSLongitude()
        {
            var grs = GRSLongitude;

            IsGRSSectionEnabled = false;
            double jd = grs.Epoch;
            double longitude = grs.Longitude;
            double drift = grs.MonthlyDrift;
            string error = null;

            await Task.Run(() =>
            {
                string tempFile = null;
                try
                {
                    tempFile = Path.GetTempFileName();

                    Downloader.Download(new Uri("https://www.ap-i.net/pub/virtualplanet/grs.txt"), tempFile);

                    Dictionary<string, string> data = File.ReadAllLines(tempFile)
                        .Skip(1)
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .Select(line => line.Split('='))
                        .ToDictionary(pair => pair[0], pair => pair[1]);

                    longitude = double.Parse(data["RefGRSLon"], CultureInfo.InvariantCulture);
                    drift = double.Parse(data["RefGRSdrift"], CultureInfo.InvariantCulture) / 12;
                    int year = int.Parse(data["RefGRSY"]);
                    int month = int.Parse(data["RefGRSM"]);
                    int day = int.Parse(data["RefGRSD"]);
                    jd = new Date(year, month, day).ToJulianDay();
                }
                catch (Exception ex)
                {
                    error = $"Unable to update GRS data. Reason: {ex.Message}";
                    Log.Error(error);
                }
                finally
                {
                    if (tempFile != null)
                    {
                        try
                        {
                            File.Delete(tempFile);
                        }
                        catch { }
                    }

                    if (error == null)
                    {
                        grs.Epoch = jd;
                        grs.MonthlyDrift = drift;
                        grs.Longitude = longitude;

                        GRSLongitude = grs;

                        NotifyPropertyChanged(nameof(GRSLongitude));
                    }
                }
            });

            IsGRSSectionEnabled = true;

            if (error != null)
            {
                ViewManager.ShowMessageBox("$Error", error);
            }
        }

        private void UpdateGenericMoonsOrbitalElements()
        {
            orbitalElementsManager.Update(calc.GenericMoons.Select(x => x.Data), OnBeforeUpdate, OnAfterUpdate);
        }

        private void OnBeforeUpdate()
        {
            OrbitalElementsIsReady = false;
        }

        private void OnAfterUpdate()
        {
            NotifyPropertyChanged(nameof(GenericMoonsOrbitalElementsLastUpdated));
            OrbitalElementsIsReady = true;
        }
    }
}
