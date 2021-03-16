using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Astrarium.Plugins.JupiterMoons.ImportExport
{
    /// <summary>
    /// Base class for CSV writers.
    /// </summary>
    /// <typeparam name="T">Type of a record to be written as a single line in CSV file.</typeparam>
    public abstract class CsvWriterBase<T> where T : class
    {
        /// <summary>
        /// Gets dictionary of record columns, key is a column name, 
        /// value is a function that provides serialized value of the column for a record.
        /// </summary>
        protected abstract Dictionary<string, Func<T, string>> Columns { get; }

        /// <summary>
        /// Writes records to the file.
        /// </summary>
        /// <param name="file">Full path to the file.</param>
        /// <param name="records">Collection of records to be written.</param>
        public void Write(string file, ICollection<T> records)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;

            using (var writer = File.CreateText(file))
            {
                var keys = Columns.Keys;
                var values = Columns.Values;
                
                // header
                writer.WriteLine(string.Join(",", keys.Select(k => $"\"{k}\"")));

                // content rows
                for (int i = 0; i < records.Count; i++)
                {
                    writer.WriteLine(string.Join(",", values.Select(v => $"\"{v.Invoke(records.ElementAt(i))}\"")));
                }

                writer.Flush();
                writer.Close();
            }
        }
    }
}
