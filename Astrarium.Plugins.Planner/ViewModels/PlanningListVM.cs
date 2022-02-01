using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Planner.ViewModels
{
    public class PlanningListVM : ViewModelBase
    {
        private readonly ObservationPlanner planner;

        public ICollection<Ephemerides> Ephemerides { get; private set; }
        public DataTable PlanTable { get; private set; }
       
        public PlanningListVM(ObservationPlanner planner)
        {
            this.planner = planner;
        }

        public async void CreatePlan(PlanningFilter filter)
        {
            var tokenSource = new CancellationTokenSource();
            var progress = new Progress<double>();

            ViewManager.ShowProgress("Please wait", "Creating observation plan...", tokenSource, progress);

            ICollection<Ephemerides> ephemerides = await Task.Run(() => planner.CreatePlan(filter, tokenSource.Token, progress));

            if (!tokenSource.IsCancellationRequested)
            {
                if (ephemerides.Any())
                {
                    FillTable(ephemerides);
                }
                tokenSource.Cancel();
            }
        }

        private void FillTable(ICollection<Ephemerides> ephemerides)
        {
            var table = new DataTable();

            var objectColumn = new DataColumn()
            {
                Caption = "Celestial object",
                ColumnName = "Object",
                DataType = typeof(string),
                ReadOnly = true
            };

            table.Columns.Add(objectColumn);
            table.Columns.AddRange(ephemerides.ElementAt(0).Select(e =>
            {
                var column = new DataColumn()
                {
                    Caption = e.Key,
                    ColumnName = e.Key,
                    DataType = e.Value?.GetType() ?? typeof(object),
                    ReadOnly = true
                };
                column.ExtendedProperties["Formatter"] = e.Formatter;
                
                return column;
            }).ToArray());

            for (int i = 0; i < ephemerides.Count; i++)
            {
                var row = table.NewRow();
                row["Object"] = string.Join(",", ephemerides.ElementAt(i).CelestialObject.Names);
                foreach (var e in ephemerides.ElementAt(i))
                {
                    row[e.Key] = e.Value ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }

            
            PlanTable = table;
            NotifyPropertyChanged(nameof(PlanTable));
            Ephemerides = ephemerides;
        }
    }
}
