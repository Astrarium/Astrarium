using ADK;
using Planetarium.Objects;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Planetarium.ViewModels
{
    public class PhenomenaVM : ViewModelBase
    {
        public Command SaveToFileCommand { get; private set; }
        public Command CloseCommand { get; private set; }
        public Command<double> SetJulianDayCommand { get; private set; }
        public double JulianDay { get; private set; }
        public IEnumerable<IGrouping<string, AstroEventVM>> Events { get; private set; }

        private ICollection<AstroEvent> events;
        private readonly IViewManager viewManager;
        private readonly ISky sky;
        
        public PhenomenaVM(IViewManager viewManager, ISky sky)
        {
            this.viewManager = viewManager;
            this.sky = sky;

            SaveToFileCommand = new Command(SaveToFile);
            CloseCommand = new Command(Close);
            SetJulianDayCommand = new Command<double>(SetJulianDay);
        }

        public void SetEvents(ICollection<AstroEvent> events)
        {
            this.events = events;
            Events = events
                .Select(e => new AstroEventVM(e, sky.Context.GeoLocation.UtcOffset))
                .GroupBy(e => e.Date);
        }

        private void SaveToFile()
        {
            var file = viewManager.ShowSaveFileDialog("Save to file", "Phenomena", ".csv", "Text files (*.txt)|*.txt|Comma-separated files (*.csv)|*.csv");
            if (file != null)
            {
                IAstroEventsWriter writer = null;
                string ext = Path.GetExtension(file);
                switch (ext)
                {
                    case ".csv":
                        writer = new AstroEventsCsvWriter(file, sky.Context.GeoLocation.UtcOffset);
                        break;
                    case ".txt":
                        writer = new AstroEventsTextWriter(file, sky.Context.GeoLocation.UtcOffset);
                        break;
                    default:
                        break;
                }

                writer?.Write(events);

                viewManager.ShowMessageBox("Information", "Export has been successfully completed.", MessageBoxButton.OK);
            }
        }

        private void SetJulianDay(double jd)
        {
            JulianDay = jd;
            Close(true);
        }
    }

    public class AstroEventVM
    {
        public string Date { get; set; }
        public string Time { get; set; }
        public double JulianDay { get; set; }
        public string Text { get; set; }
        public bool NoExactTime { get; set; }

        public AstroEventVM(AstroEvent e, double utcOffset)
        {
            var date = new Date(e.JulianDay, utcOffset);
            JulianDay = e.JulianDay;
            Text = e.Text;
            NoExactTime = e.NoExactTime;
            Date = Formatters.DateOnly.Format(date);
            Time = Formatters.TimeOnly.Format(date);
        }
    }

    public interface IAstroEventsWriter
    {
        void Write(ICollection<AstroEvent> events);
    }

    public class AstroEventsTextWriter : IAstroEventsWriter
    {
        private string file;
        private double utcOffset;

        public AstroEventsTextWriter(string file, double utcOffset)
        {
            this.file = file;
            this.utcOffset = utcOffset;
        }

        public void Write(ICollection<AstroEvent> events)
        {
            using (var writer = File.CreateText(file))
            {
                foreach (var e in events)
                {
                    var date = new Date(e.JulianDay, utcOffset);
                    string dateString = Formatters.DateOnly.Format(date);
                    string timeString = e.NoExactTime ? "     " : Formatters.TimeOnly.Format(date);
                    writer.WriteLine($"{dateString} {timeString} {e.Text}");
                }
                writer.Flush();
                writer.Close();
            }            
        }
    }

    public class AstroEventsCsvWriter : IAstroEventsWriter
    {
        private string file;
        private double utcOffset;

        public AstroEventsCsvWriter(string file, double utcOffset)
        {
            this.file = file;
            this.utcOffset = utcOffset;
        }

        public void Write(ICollection<AstroEvent> events)
        {
            using (var writer = File.CreateText(file))
            {
                foreach (var e in events)
                {
                    var date = new Date(e.JulianDay, utcOffset);
                    string dateTimeString = Formatters.DateOnly.Format(date);
                    if (!e.NoExactTime) dateTimeString += $" {Formatters.TimeOnly.Format(date)}";
                    writer.WriteLine($"\"{e.JulianDay}\",\"{dateTimeString}\",\"{e.Text}\"");
                }
                writer.Flush();
                writer.Close();
            }
        }
    }
}
