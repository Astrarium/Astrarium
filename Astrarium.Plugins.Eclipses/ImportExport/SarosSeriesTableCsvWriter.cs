using Astrarium.Plugins.Eclipses.Types;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Astrarium.Plugins.Eclipses.ImportExport
{
    /// <summary>
    /// CSV writer for list of saros series.
    /// </summary>
    public class SarosSeriesTableCsvWriter : CsvWriterBase<SarosSeriesTableItem>
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
        /// Creates new instance of the writer.
        /// </summary>
        /// <param name="isRawData">Flag indicating raw (unformatted) data should be serialized.</param>
        public SarosSeriesTableCsvWriter(bool isRawData)
        {
            this.isRawData = isRawData;
        }

        /// <inheritdoc/>
        protected override Dictionary<string, Func<SarosSeriesTableItem, string>> Columns => 
            new Dictionary<string, Func<SarosSeriesTableItem, string>>()
        {    
            ["Member"] = i => i.Member,
            ["Date"] = i => isRawData ? i.JulianDay.ToString(ci) : i.Date,
            ["Type"] = i => i.Type,
            ["Gamma"] = i => i.Gamma,
            ["Magnitude"] = i => i.Magnitude,
            ["LocalVisibility"] = i => i.LocalVisibility
        };
    }
}
