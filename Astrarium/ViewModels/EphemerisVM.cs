using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace Astrarium.ViewModels
{
    public class EphemerisVM : ViewModelBase
    {
        public string Header { get; private set; }
        public DataTable EphemerisTable { get; private set; }      
        public Command SaveToFileCommand { get; private set; }
        public Command CloseCommand { get; private set; }

        private readonly ISky sky;
        private CelestialObject Body;
        private List<List<Ephemeris>> Ephemeris;
        private double StartDate;
        private double EndDate;
        private TimeSpan Step;

        public EphemerisVM(ISky sky)
        {
            this.sky = sky;

            SaveToFileCommand = new Command(SaveToFile);
            CloseCommand = new Command(Close);
        }

        public void SetData(CelestialObject body, double jdFrom, double jdTo, TimeSpan step, List<List<Ephemeris>> ephemeris)
        {
            string bodyName = body.Names.First();
            string bodyTypeName = body.GetType().Name;
            double utcOffset = sky.Context.GeoLocation.UtcOffset;

            Header = Text.Get("EphemeridesWindow.Header",
                ("objectName", bodyName),
                ("startDate", Formatters.DateTime.Format(new Date(jdFrom, utcOffset))),
                ("endDate", Formatters.DateTime.Format(new Date(jdTo, utcOffset))),
                ("step", Formatters.TimeSpan.Format(step))
            );
           
            var table = new DataTable();
            table.Columns.Add(new DataColumn() { Caption = Text.Get("EphemeridesWindow.DateColumn"), ColumnName = "Date" });
            table.Columns.AddRange(ephemeris[0].Select(e => new DataColumn() { Caption = Text.Get($"{bodyTypeName}.{e.Key}"), ColumnName = e.Key }).ToArray());

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

            Body = body;
            Ephemeris = ephemeris;
            EphemerisTable = table;
            StartDate = jdFrom;
            EndDate = jdTo;
            Step = step;
        }

        private void SaveToFile()
        {
            var file = ViewManager.ShowSaveFileDialog(Text.Get("EphemeridesWindow.ExportTitle"), "Ephemerides", ".csv", "Comma-separated files (*.csv)|*.csv");
            if (file != null)
            {
                IEphemeridesWriter writer = null;
                string ext = Path.GetExtension(file);
                switch (ext)
                {
                    case ".csv":
                        writer = new EphemeridesCsvWriter(file, sky.Context.GeoLocation.UtcOffset);
                        break;
                    default:
                        break;
                }

                writer?.Write(Body, StartDate, EndDate, Step, Ephemeris);

                ViewManager.ShowMessageBox(Text.Get("EphemeridesWindow.ExportDoneTitle"), Text.Get("EphemeridesWindow.ExportDoneText"), MessageBoxButton.OK);
            }
        }
    }

    public interface IEphemeridesWriter
    {
        void Write(CelestialObject body, double jdFrom, double jdTo, TimeSpan step, List<List<Ephemeris>> ephemerides);
    }

    public class EphemeridesCsvWriter : IEphemeridesWriter
    {
        private string file;
        private double utcOffset;

        public EphemeridesCsvWriter(string file, double utcOffset)
        {
            this.file = file;
            this.utcOffset = utcOffset;
        }

        public void Write(CelestialObject body, double jdFrom, double jdTo, TimeSpan step, List<List<Ephemeris>> ephemerides)
        {
            string bodyTypeName = body.GetType().Name;

            StringBuilder row = new StringBuilder();

            using (var writer = File.CreateText(file))
            {
                // header
                row.Append("\"");
                row.Append(Text.Get("EphemeridesWindow.DateColumn"));
                row.Append("\", ");
                row.Append(string.Join(", ", ephemerides[0].Select(e => $"\"{Text.Get($"{bodyTypeName}.{e.Key}")}\"")));
                writer.WriteLine(row.ToString());

                // content rows
                for (int i = 0; i < ephemerides.Count; i++)
                {
                    row = new StringBuilder();
                    row.Append("\"");
                    row.Append(Formatters.DateTime.Format(new Date(jdFrom + i * step.TotalDays, utcOffset)));
                    row.Append("\",");

                    foreach (var e in ephemerides[i])
                    {
                        row.Append("\"");
                        row.Append(e.Formatter.Format(e.Value));
                        row.Append("\",");
                    }

                    writer.WriteLine(row.ToString().TrimEnd(','));
                }

                writer.Flush();
                writer.Close();
            }
        }
    }
}
