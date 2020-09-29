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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Astrarium.Plugins.SolarSystem.Controls
{
    /// <summary>
    /// Interaction logic for GRSSettingControl.xaml
    /// </summary>
    public partial class GRSSettingControl : UserControl
    {
        private object locker = new object();

        public GRSSettingControl()
        {
            InitializeComponent();
        }

        private async void UpdateFromServer(object sender, RoutedEventArgs e)
        {            
            IsEnabled = false;
            double jd = Epoch.JulianDay;
            decimal longitude = Longitude.Value;
            decimal drift = MonthlyDrift.Value;
            bool isError = false;

            await Task.Run(() =>
            {
                string tempFile = null;
                try
                {
                    tempFile = Path.GetTempFileName();
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("https://www.ap-i.net/pub/virtualplanet/grs.txt", tempFile);
                    }

                    Dictionary<string, string> data = File.ReadAllLines(tempFile)
                        .Skip(1)
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .Select(line => line.Split('='))
                        .ToDictionary(pair => pair[0], pair => pair[1]);

                    longitude = decimal.Parse(data["RefGRSLon"], CultureInfo.InvariantCulture);
                    drift = decimal.Parse(data["RefGRSdrift"], CultureInfo.InvariantCulture) / 12;
                    int year = int.Parse(data["RefGRSY"]);
                    int month = int.Parse(data["RefGRSM"]);
                    int day = int.Parse(data["RefGRSD"]);
                    jd = new Date(year, month, day).ToJulianDay();
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Unable to update GRS data. Reason: {ex}");
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

            IsEnabled = true;

            if (isError)
            {
                ViewManager.ShowMessageBox("$Error", "Unable to update GRS data.");
            }
            else
            {
                Epoch.JulianDay = jd;
                MonthlyDrift.Value = drift;
                Longitude.Value = longitude;                
            }
        }
    }
}
