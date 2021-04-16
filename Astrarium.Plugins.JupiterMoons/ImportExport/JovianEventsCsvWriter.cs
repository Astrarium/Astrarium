using System;
using System.Collections.Generic;
using System.Globalization;

namespace Astrarium.Plugins.JupiterMoons.ImportExport
{
    /// <summary>
    /// CSV writer for list of events of Jupiter moons system.
    /// </summary>
    public class JovianEventsCsvWriter : CsvWriterBase<EventsTableItem>
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
        public JovianEventsCsvWriter(bool isRawData)
        {
            this.isRawData = isRawData;
        }

        /// <inheritdoc/>
        protected override Dictionary<string, Func<EventsTableItem, string>> Columns 
        {
            get
            {
                if (isRawData)
                {
                    return new Dictionary<string, Func<EventsTableItem, string>>
                    {
                        ["Begin"] = i => i.Event.JdBegin.ToString(ci),
                        ["End"] = i => i.Event.JdEnd.ToString(ci),
                        ["Duration"] = i => i.Event.Duration.ToString(ci),
                        ["Event"] = i => i.Text,
                        ["Code"] = i => i.Code,
                        ["JupAltBegin"] = i => i.Event.JupiterAltBegin.ToString(ci),
                        ["JupAltEnd"] = i => i.Event.JupiterAltEnd.ToString(ci),
                        ["SunAltBegin"] = i => i.Event.SunAltBegin.ToString(ci),
                        ["SunAltEnd"] = i => i.Event.SunAltEnd.ToString(ci),
                        ["Notes"] = i => i.Notes,
                    };
                }
                else
                {
                    return new Dictionary<string, Func<EventsTableItem, string>>
                    {
                        ["BeginDate"] = i => i.BeginDate,
                        ["BeginTime"] = i => i.BeginTime,
                        ["EndTime"] = i => i.EndTime,
                        ["Duration"] = i => i.Duration,
                        ["Event"] = i => i.Text,
                        ["Code"] = i => i.Code,
                        ["JupAltBegin"] = i => i.JupiterAltBegin,
                        ["JupAltEnd"] = i => i.JupiterAltEnd,
                        ["SunAltBegin"] = i => i.SunAltBegin,
                        ["SunAltEnd"] = i => i.SunAltEnd,
                        ["Notes"] = i => i.Notes,
                    };
                }
            }
        }
    }
}

