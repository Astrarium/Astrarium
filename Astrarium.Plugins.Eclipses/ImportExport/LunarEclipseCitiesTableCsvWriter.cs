using Astrarium.Plugins.Eclipses.Types;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Astrarium.Plugins.Eclipses.ImportExport
{
    /// <summary>
    /// CSV writer for list of local circumstances of lunar eclipses.
    /// </summary>
    public class LunarEclipseCitiesTableCsvWriter : CsvWriterBase<LunarEclipseCitiesListTableItem>
    {
        /// <summary>
        /// Flag indicating raw (unformatted) data should be serialized.
        /// </summary>
        private readonly bool isRawData;

        /// <summary>
        /// Invariant culture used for serialization.
        /// </summary>
        private readonly CultureInfo ci = CultureInfo.InvariantCulture;

        /// <summary>
        /// Creates new instance of the writer.
        /// </summary>
        /// <param name="isRawData">Flag indicating raw (unformatted) data should be serialized.</param>
        public LunarEclipseCitiesTableCsvWriter(bool isRawData)
        {
            this.isRawData = isRawData;
        }

        /// <inheritdoc/>
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

