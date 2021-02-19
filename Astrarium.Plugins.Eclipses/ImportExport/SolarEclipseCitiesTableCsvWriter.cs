using Astrarium.Plugins.Eclipses.Types;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Astrarium.Plugins.Eclipses.ImportExport
{
    public class SolarEclipseCitiesTableCsvWriter : CsvWriterBase<SolarEclipseCitiesListTableItem>
    {
        private bool isRawData;
        private CultureInfo ci = CultureInfo.InvariantCulture;
        private string DurToString(double? dur) => dur != null ? (dur.Value * 24).ToString(ci) : "";

        public SolarEclipseCitiesTableCsvWriter(string file, bool isRawData) : base(file)
        {
            this.isRawData = isRawData;
        }

        protected override Dictionary<string, Func<SolarEclipseCitiesListTableItem, string>> Columns => 
            new Dictionary<string, Func<SolarEclipseCitiesListTableItem, string>>
            {
                ["LocationName"] = i => i.LocationName,
                ["Latitude"] = i => i.Location.Latitude.ToString(ci),
                ["Longitude"] = i => (-i.Location.Longitude).ToString(ci),
                ["TimeZone"] = i => isRawData ? i.TimeZone.ToString(ci) : i.TimeZoneString,
                ["MaxMag"] = i => isRawData ? i.MaxMag.ToString(ci) : i.MaxMagString,
                ["MoonSunRatio"] = i => isRawData ? i.MoonSunRatio.ToString(ci) : i.MoonSunRatioString,
                ["PartialDur"] = i => isRawData ? DurToString(i.PartialDur) : i.PartialDurString,
                ["TotalDur"] = i => isRawData ? DurToString(i.TotalDur) : i.TotalDurString,
                ["ShadowWidth"] = i => isRawData ? i.ShadowWidth?.ToString(ci) : i.ShadowWidthString,
                ["C1Time"] = i => isRawData ? i.C1Time?.ToString(ci) : i.C1TimeString,
                ["C2Time"] = i => isRawData ? i.C2Time?.ToString(ci) : i.C2TimeString,
                ["MaxTime"] = i => isRawData ? i.MaxTime?.ToString(ci) : i.MaxTimeString,
                ["C3Time"] = i => isRawData ? i.C3Time?.ToString(ci) : i.C3TimeString,
                ["C4Time"] = i => isRawData ? i.C4Time?.ToString(ci) : i.C4TimeString,
                ["Visibility"] = i => i.Visibility
            };
    }
}

