using ADK;
using Planetarium.Objects;
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
        private readonly Sky sky;

        public EphemerisVM(IViewManager viewManager, Sky sky)
        {
            this.viewManager = viewManager;
            this.sky = sky;

            SaveToFileCommand = new Command(SaveToFile);
            CloseCommand = new Command(Close);
        }

        public void SetData(CelestialObject body, double jdFrom, double jdTo, double step, List<List<Ephemeris>> ephemeris)
        {
            string bodyName = sky.GetObjectName(body);
            double utcOffset = sky.Context.GeoLocation.UtcOffset;

            Header = $"Ephemeris of {bodyName}\nStart date: {Formatters.DateTime.Format(new Date(jdFrom, utcOffset))}\nEnd date: {Formatters.DateTime.Format(new Date(jdTo, utcOffset))}\nStep: {step}";

            var table = new DataTable();
            table.Columns.AddRange(ephemeris[0].Select(e => new DataColumn() { Caption = e.Key, ColumnName = e.Key }).ToArray());

            for (int i = 0; i < ephemeris.Count; i++)
            {
                table.Rows.Add(ephemeris[i].Select(e => e.Formatter.Format(e.Value)).ToArray());
            }

            EphemerisTable = table;
        }

        private void SaveToFile()
        {
            var result = viewManager.ShowSaveFileDialog("Save to file", "Ephemeris", ".csv", "Text files (*.txt)|*.txt|Comma-separated files (*.csv)|*.csv");
            if (result != null)
            {
                
            }
        }
    }
}
