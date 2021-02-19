using Astrarium.Plugins.Eclipses.Types;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Astrarium.Plugins.Eclipses.ImportExport
{
    public class LunarEclipseCitiesTableCsvWriter : CsvWriterBase<LunarEclipseCitiesListTableItem>
    {
        private bool isRawData;
        private CultureInfo ci = CultureInfo.InvariantCulture;
        private string DurToString(double? dur) => dur != null ? (dur.Value * 24).ToString(ci) : "";

        public LunarEclipseCitiesTableCsvWriter(string file, bool isRawData) : base(file)
        {
            this.isRawData = isRawData;
        }

        protected override Dictionary<string, Func<LunarEclipseCitiesListTableItem, string>> Columns => 
            new Dictionary<string, Func<LunarEclipseCitiesListTableItem, string>>
            {
                ["LocationName"] = i => i.LocationName,
                ["Latitude"] = i => i.Location.Latitude.ToString(ci),
                ["Longitude"] = i => (-i.Location.Longitude).ToString(ci),
                ["TimeZone"] = i => isRawData ? i.TimeZone.ToString(ci) : i.TimeZoneString,
                ["Visibility"] = i => i.Visibility
            };
    }
}

