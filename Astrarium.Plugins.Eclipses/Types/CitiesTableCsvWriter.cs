using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Astrarium.Plugins.Eclipses.ViewModels.SolarEclipseVM;

namespace Astrarium.Plugins.Eclipses.Types
{
    public class CitiesTableCsvWriter
    {
        private string file;
        private bool isRawData;

        public CitiesTableCsvWriter(string file, bool isRawData)
        {
            this.file = file;
            this.isRawData = isRawData;
        }

        public void Write(ICollection<CitiesListTableItem> list)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;

            using (var writer = File.CreateText(file))
            {
                Func<double?, string> durToString = (dur) => dur != null ? (dur.Value * 24).ToString(ci) : "";

                var columns = new Dictionary<string, Func<CitiesListTableItem, string>>
                {
                    ["LocationName"] = i => i.LocationName,
                    ["Latitude"] = i => i.Location.Latitude.ToString(ci),
                    ["Longitude"] = i => (-i.Location.Longitude).ToString(ci),
                    ["TimeZone"] = i => isRawData ? i.TimeZone.ToString(ci) : i.TimeZoneString,
                    ["MaxMag"] = i => isRawData ? i.MaxMag.ToString(ci) : i.MaxMagString,
                    ["MoonSunRatio"] = i => isRawData ? i.MoonSunRatio.ToString(ci) : i.MoonSunRatioString,
                    ["PartialDur"] = i => isRawData ? durToString(i.PartialDur) : i.PartialDurString,
                    ["TotalDur"] = i => isRawData ? durToString(i.TotalDur) : i.TotalDurString,
                    ["ShadowWidth"] = i => isRawData ? i.ShadowWidth?.ToString(ci) : i.ShadowWidthString,
                    ["C1Time"] = i => isRawData ? i.C1Time?.ToString(ci) : i.C1TimeString,
                    ["C2Time"] = i => isRawData ? i.C2Time?.ToString(ci) : i.C2TimeString,
                    ["MaxTime"] = i => isRawData ? i.MaxTime?.ToString(ci) : i.MaxTimeString,
                    ["C3Time"] = i => isRawData ? i.C3Time?.ToString(ci) : i.C3TimeString,
                    ["C4Time"] = i => isRawData ? i.C4Time?.ToString(ci) : i.C4TimeString,
                    ["Visibility"] = i => i.Visibility
                };

                // header
                writer.WriteLine(string.Join(",", columns.Keys.Select(k => $"\"{k}\"")));

                // content rows
                for (int i = 0; i < list.Count; i++)
                {
                    writer.WriteLine(string.Join(",", columns.Values.Select(v => $"\"{v.Invoke(list.ElementAt(i))}\"")));
                }

                writer.Flush();
                writer.Close();
            }
        }
    }
}
