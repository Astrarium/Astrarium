using Astrarium.Algorithms;
using Astrarium.Objects;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Astrarium.ViewModels
{
    public class PhenomenaVM : ViewModelBase
    {
        public Command SaveToFileCommand { get; private set; }
        public Command CloseCommand { get; private set; }
        public Command<AstroEventVM> SelectAstroEventCommand { get; private set; }
        public double JulianDay { get; private set; }
        public CelestialObject Body { get; private set; }
        public IEnumerable<IGrouping<string, AstroEventVM>> Events { get; private set; }

        private ICollection<AstroEvent> events;
        private readonly ISky sky;
        
        public PhenomenaVM(ISky sky)
        {
            this.sky = sky;

            SaveToFileCommand = new Command(SaveToFile);
            CloseCommand = new Command(Close);
            SelectAstroEventCommand = new Command<AstroEventVM>(SelectAstroEvent);
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
            var file = ViewManager.ShowSaveFileDialog("Save to file", "Phenomena", ".csv", "Text files (*.txt)|*.txt|Comma-separated files (*.csv)|*.csv");
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

                ViewManager.ShowMessageBox("Information", "Export has been successfully completed.", MessageBoxButton.OK);
            }
        }

        private void SelectAstroEvent(AstroEventVM ev)
        {
            JulianDay = ev.JulianDay;
            Body = ev.Body;
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
        public CelestialObject Body { get; set; }

        public AstroEventVM(AstroEvent e, double utcOffset)
        {
            var date = new Date(e.JulianDay, utcOffset);
            JulianDay = e.JulianDay;
            Text = e.Text;
            NoExactTime = e.NoExactTime;
            Body = e.Body;
            Date = Formatters.Date.Format(date);
            Time = Formatters.Time.Format(date);
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
                    string dateString = Formatters.Date.Format(date);
                    string timeString = e.NoExactTime ? "     " : Formatters.Time.Format(date);
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
                    string dateTimeString = Formatters.Date.Format(date);
                    if (!e.NoExactTime) dateTimeString += $" {Formatters.Time.Format(date)}";
                    writer.WriteLine($"\"{e.JulianDay}\",\"{dateTimeString}\",\"{e.Text}\"");
                }
                writer.Flush();
                writer.Close();
            }
        }
    }
}
