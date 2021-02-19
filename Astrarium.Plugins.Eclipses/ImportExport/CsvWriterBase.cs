using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Astrarium.Plugins.Eclipses.ImportExport
{
    public abstract class CsvWriterBase<T> where T : class
    {
        protected abstract Dictionary<string, Func<T, string>> Columns { get; }

        protected string file;

        protected CsvWriterBase(string file) 
        {
            this.file = file;
        }

        public void Write(ICollection<T> list)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;

            using (var writer = File.CreateText(file))
            {
                var keys = Columns.Keys;
                var values = Columns.Values;
                
                // header
                writer.WriteLine(string.Join(",", keys.Select(k => $"\"{k}\"")));

                // content rows
                for (int i = 0; i < list.Count; i++)
                {
                    writer.WriteLine(string.Join(",", values.Select(v => $"\"{v.Invoke(list.ElementAt(i))}\"")));
                }

                writer.Flush();
                writer.Close();
            }
        }
    }
}
