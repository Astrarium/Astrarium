using Astrarium.Plugins.Eclipses.Types;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Astrarium.Plugins.Eclipses.ImportExport
{
    /// <summary>
    /// CSV writer for list of local circumstances of solar eclipses.
    /// </summary>
    public class SolarEclipseCitiesTableCsvWriter : CsvWriterBase<SolarEclipseCitiesListTableItem>
    {
        /// <summary>
        /// Flag indicating raw (unformatted) data should be serialized.
        /// </summary>
        private bool isRawData;

        /// <summary>
        /// Invariant culture used for serialization.
        /// </summary>
        private CultureInfo ci = CultureInfo.InvariantCulture;
        
        /// <summary>
        /// Converter to serialize duration to string.
        /// </summary>
        /// <param name="dur">Duration of an event, in fractions of day.</param>
        /// <returns></returns>
        private string DurToString(double? dur) => dur != null ? (dur.Value * 24).ToString(ci) : "";

        /// <summary>
        /// Creates new instance of the writer.
        /// </summary>
        /// <param name="isRawData">Flag indicating raw (unformatted) data should be serialized.</param>
        public SolarEclipseCitiesTableCsvWriter(bool isRawData)
        {
            this.isRawData = isRawData;
        }

        /// <inheritdoc/>
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

