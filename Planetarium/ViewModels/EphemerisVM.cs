using ADK;
using Planetarium.Objects;
using Planetarium.Types;
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
        public string Header { get; private set; }
        public DataTable EphemerisTable { get; private set; }

        public Command SaveToFileCommand { get; private set; }
        public Command CloseCommand { get; private set; }

        private readonly IViewManager viewManager;
        private readonly ISky sky;

        public EphemerisVM(IViewManager viewManager, ISky sky)
        {
            this.viewManager = viewManager;
            this.sky = sky;

            SaveToFileCommand = new Command(SaveToFile);
            CloseCommand = new Command(Close);
        }

        public void SetData(CelestialObject body, double jdFrom, double jdTo, TimeSpan step, List<List<Ephemeris>> ephemeris)
        {
            string bodyName = sky.GetObjectName(body);
            double utcOffset = sky.Context.GeoLocation.UtcOffset;

            string headerTemplate = "Ephemerides of {0}\nStart date: {1}\nEnd date: {2}\nStep: {3}";

            Header = string.Format(headerTemplate,
                bodyName,
                Formatters.DateTime.Format(new Date(jdFrom, utcOffset)),
                Formatters.DateTime.Format(new Date(jdTo, utcOffset)),
                Formatters.TimeSpan.Format(step));
           
            var table = new DataTable();
            table.Columns.Add(new DataColumn() { Caption = "Date", ColumnName = "Date" });
            table.Columns.AddRange(ephemeris[0].Select(e => new DataColumn() { Caption = e.Key, ColumnName = e.Key }).ToArray());

            for (int i = 0; i < ephemeris.Count; i++)
            {
                var row = table.NewRow();
                row["Date"] = Formatters.DateTime.Format(new Date(jdFrom + i * step.TotalDays, utcOffset));
                foreach (var e in ephemeris[i])
                {
                    row[e.Key] = e.Formatter.Format(e.Value);
                }
                table.Rows.Add(row);
            }

            EphemerisTable = table;
        }

        private void SaveToFile()
        {
            var result = viewManager.ShowSaveFileDialog("Save to file", "Ephemerides", ".csv", "Text files (*.txt)|*.txt|Comma-separated files (*.csv)|*.csv");
            if (result != null)
            {
                
            }
        }
    }
}
