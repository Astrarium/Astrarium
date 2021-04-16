using System;
using System.Collections.Generic;
using System.Globalization;

namespace Astrarium.Plugins.JupiterMoons.ImportExport
{
    /// <summary>
    /// CSV writer for list of events of Great Red Spot of Jupiter.
    /// </summary>
    public class GRSEventsCsvWriter : CsvWriterBase<GRSTableItem>
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
        public GRSEventsCsvWriter(bool isRawData)
        {
            this.isRawData = isRawData;
        }

        /// <inheritdoc/>
        protected override Dictionary<string, Func<GRSTableItem, string>> Columns 
        {
            get
            {
                if (isRawData)
                {
                    return new Dictionary<string, Func<GRSTableItem, string>>
                    {
                        ["Transit"] = i => i.Event.JdTransit.ToString(ci),
                        ["Appear"] = i => i.Event.JdAppear.ToString(ci),
                        ["Disappear"] = i => i.Event.JdDisappear.ToString(ci),                        
                        ["JupAltTransit"] = i => i.Event.JupiterAltTransit.ToString(ci),                       
                        ["SunAltTransit"] = i => i.Event.SunAltTransit.ToString(ci),
                        ["JupAltAppear"] = i => i.Event.JupiterAltAppear.ToString(ci),
                        ["SunAltAppear"] = i => i.Event.SunAltAppear.ToString(ci),
                        ["JupAltDisappear"] = i => i.Event.JupiterAltDisappear.ToString(ci),
                        ["SunAltDisappear"] = i => i.Event.SunAltDisappear.ToString(ci),
                    };
                }
                else
                {
                    return new Dictionary<string, Func<GRSTableItem, string>>
                    {
                        ["Date"] = i => i.Date,
                        ["Transit"] = i => i.TransitTime,
                        ["Appear"] = i => i.AppearTime,
                        ["Disappear"] = i => i.DisappearTime,                       
                        ["JupAltTransit"] = i => i.JupiterAltTransit,
                        ["SunAltTransit"] = i => i.SunAltTransit,
                        ["JupAltAppear"] = i => i.JupiterAltAppear,
                        ["SunAltAppear"] = i => i.SunAltAppear,
                        ["JupAltDisappear"] = i => i.JupiterAltDisappear,
                        ["SunAltDisappear"] = i => i.SunAltDisappear,
                    };
                }
            }
        }
    }
}

