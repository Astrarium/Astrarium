using Astrarium.Plugins.Eclipses.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Astrarium.Plugins.Eclipses.ImportExport
{
    public class SarosSeriesTableCsvWriter
    {
        private string file;
        private bool isRawData;

        public SarosSeriesTableCsvWriter(string file, bool isRawData)
        {
            this.file = file;
            this.isRawData = isRawData;
        }

        public void Write(ICollection<SarosSeriesTableItem> list)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;

            using (var writer = File.CreateText(file))
            {
                Func<double?, string> durToString = (dur) => dur != null ? (dur.Value * 24).ToString(ci) : "";

                var columns = new Dictionary<string, Func<SarosSeriesTableItem, string>>
                {
                    ["Member"] = i => i.Member,
                    ["Date"] = i => isRawData ? i.JulianDay.ToString(ci) : i.Date,
                    ["Type"] = i => i.Type,
                    ["Gamma"] = i => i.Gamma,
                    ["Magnitude"] = i => i.Magnitude,
                    ["LocalVisibility"] = i => i.LocalVisibility
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
