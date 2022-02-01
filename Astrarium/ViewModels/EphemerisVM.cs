using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
        private List<Ephemerides> Ephemeris;
        private double StartDate;
        private double EndDate;
        private TimeSpan Step;

        public EphemerisVM(ISky sky)
        {
            this.sky = sky;

            SaveToFileCommand = new Command(SaveToFile);
            CloseCommand = new Command(Close);
        }

        public void SetData(CelestialObject body, double jdFrom, double jdTo, TimeSpan step, List<Ephemerides> ephemerides)
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

            var dateColumn = new DataColumn() 
            { 
                Caption = Text.Get("EphemeridesWindow.DateColumn"), 
                ColumnName = "Date", 
                DataType = typeof(Date), 
                ReadOnly = true 
            };
            dateColumn.ExtendedProperties["Formatter"] = Formatters.DateTime;
            table.Columns.Add(dateColumn);
            table.Columns.AddRange(ephemerides[0].Select(e => 
            {
                var column = new DataColumn()
                {
                    Caption = Text.Get($"{bodyTypeName}.{e.Key}"),
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
                row["Date"] = new Date(jdFrom + i * step.TotalDays, utcOffset);
                foreach (var e in ephemerides[i])
                {
                    row[e.Key] = e.Value;
                }
                table.Rows.Add(row);
            }

            Body = body;
            Ephemeris = ephemerides;
            EphemerisTable = table;
            StartDate = jdFrom;
            EndDate = jdTo;
            Step = step;
        }

        private void SaveToFile()
        {
            var formats = new Dictionary<string, string>
            {
                ["Comma-separated files (with formatting) (*.csv)"] = "*.csv",
                ["Comma-separated files (raw data) (*.csv)"] = "*.csv",
            };
            string filter = string.Join("|", formats.Select(kv => $"{kv.Key}|{kv.Value}"));
            var file = ViewManager.ShowSaveFileDialog(Text.Get("EphemeridesWindow.ExportTitle"), "Ephemerides", ".csv", filter, out int selectedFilterIndex);
            if (file != null)
            {
                IEphemeridesWriter writer = null;
                string ext = Path.GetExtension(file);
                switch (ext)
                {
                    case ".csv":
                        writer = new EphemeridesCsvWriter(file, sky.Context.GeoLocation.UtcOffset, selectedFilterIndex == 2);
                        break;
                    default:
                        break;
                }

                writer?.Write(Body, StartDate, EndDate, Step, Ephemeris);

                var answer = ViewManager.ShowMessageBox("$EphemeridesWindow.ExportDoneTitle", "$EphemeridesWindow.ExportDoneText", MessageBoxButton.YesNo);
                if (answer == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(file);
                }
            }
        }
    }

    public interface IEphemeridesWriter
    {
        void Write(CelestialObject body, double jdFrom, double jdTo, TimeSpan step, List<Ephemerides> ephemerides);
    }

    public class EphemeridesCsvWriter : IEphemeridesWriter
    {
        private string file;
        private double utcOffset;
        private bool isRawData;
        private NumberFormatInfo nf;

        public EphemeridesCsvWriter(string file, double utcOffset, bool isRawData)
        {
            this.file = file;
            this.utcOffset = utcOffset;
            this.isRawData = isRawData;
            nf = new NumberFormatInfo();
            nf.NumberDecimalSeparator = ".";
        }

        public void Write(CelestialObject body, double jdFrom, double jdTo, TimeSpan step, List<Ephemerides> ephemerides)
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
                    double jd = jdFrom + i * step.TotalDays;
                    row = new StringBuilder();
                    row.Append("\"");
                    row.Append(isRawData ? jd.ToString(nf) : Formatters.DateTime.Format(new Date(jd, utcOffset)));
                    row.Append("\",");

                    foreach (var e in ephemerides[i])
                    {
                        row.Append("\"");
                        row.Append(isRawData ? Convert.ToString(e.Value, nf) : e.Formatter.Format(e.Value));
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
