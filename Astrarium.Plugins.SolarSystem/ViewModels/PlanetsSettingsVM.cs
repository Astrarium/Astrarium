using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.SolarSystem.ViewModels
{
    public class PlanetsSettingsVM : SettingsViewModel
    {
        public Command UpdateGRSLongitudeCommand { get; private set; }

        public bool IsGRSSectionEnabled
        {
            get => GetValue<bool>(nameof(IsGRSSectionEnabled));
            set => SetValue(nameof(IsGRSSectionEnabled), value);
        }

        public PlanetsSettingsVM(ISettings settings) : base(settings)
        {
            UpdateGRSLongitudeCommand = new Command(UpdateGRSLongitude);
            IsGRSSectionEnabled = true;
        }

        private async void UpdateGRSLongitude()
        {
            var grs = Settings.Get<GreatRedSpotSettings>("GRSLongitude");

            IsGRSSectionEnabled = false;
            double jd = grs.Epoch;
            double longitude = grs.Longitude;
            double drift = grs.MonthlyDrift;
            bool isError = false;

            await Task.Run(() =>
            {
                string tempFile = null;
                try
                {
                    tempFile = Path.GetTempFileName();
                    using (var client = new WebClient())
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                        client.DownloadFile("https://www.ap-i.net/pub/virtualplanet/grs.txt", tempFile);
                    }

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
                    Log.Error($"Unable to update GRS data. Reason: {ex}");
                    isError = true;
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
                }
            });

            IsGRSSectionEnabled = true;

            if (isError)
            {
                ViewManager.ShowMessageBox("$Error", "Unable to update GRS data.");
            }
            else
            {
                grs.Epoch = jd;
                grs.MonthlyDrift = drift;
                grs.Longitude = longitude;

                Settings.Set("GRSLongitude", grs);
            }
        }
    }
}
