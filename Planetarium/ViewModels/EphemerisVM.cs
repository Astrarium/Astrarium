using ADK.Demo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.ViewModels
{
    public class EphemerisVM : ViewModelBase
    {
        public DataTable EphemerisTable { get; private set; }

        public void SetEphemeris(List<List<Ephemeris>> ephem, double from, double to, double step, double utcOffset)
        {
            var table = new DataTable();
            table.Columns.AddRange(ephem[0].Select(e => new DataColumn() { Caption = e.Key, ColumnName = e.Key }).ToArray());

            for (int i = 0; i < ephem.Count; i++)
            {
                table.Rows.Add(ephem[i].Select(e => e.Formatter.Format(e.Value)).ToArray());
            }

            EphemerisTable = table;
        }
    }
}
