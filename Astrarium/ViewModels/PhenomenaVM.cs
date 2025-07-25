using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;

namespace Astrarium.ViewModels
{
    public class PhenomenaVM : ViewModelBase
    {
        public Command SaveToFileCommand { get; private set; }
        public Command CloseCommand { get; private set; }
        public Command<AstroEventVM> SelectAstroEventCommand { get; private set; }
        public IEnumerable<IGrouping<string, AstroEventVM>> Events { get; private set; }
        public event Action<AstroEvent> OnEventSelected;

        private ICollection<AstroEvent> events;
        private readonly ISky sky;

        private readonly Dictionary<string, Type> exportFormats = new Dictionary<string, Type>()
        {
            ["Text file(*.txt)|*.txt"] = typeof(AstroEventsTextWriter),
            ["Comma-separated file with Julian dates (*.csv)|*.csv"] = typeof(AstroEventsGenericCsvWriter),
            ["Comma-separated file suitable for Google Calendar (*.csv)|*.csv"] = typeof(AstroEventsGoogleCalendarCsvWriter),
            ["iCalendar file (*.ics)|*.ics"] = typeof(AstroEventsICalendarWriter),
        };

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
            NotifyPropertyChanged(nameof(Events), nameof(NoEvents));
        }

        public bool NoEvents => !Events.Any();

        private Type GetExportFormat(int index)
        {
            return exportFormats.ElementAt(index - 1).Value;
        }

        private void SaveToFile()
        {
            var file = ViewManager.ShowSaveFileDialog("$PhenomenaWindow.ExportTitle", "Phenomena", ".ics", string.Join("|", exportFormats.Keys), out int selectedFilterIndex); ;
            if (file != null)
            {
                Type format = GetExportFormat(selectedFilterIndex);
                IAstroEventsWriter writer = (IAstroEventsWriter)Activator.CreateInstance(format);
                writer?.Write(events, file, sky.Context.GeoLocation.UtcOffset);

                var answer = ViewManager.ShowMessageBox("$PhenomenaWindow.ExportDoneTitle", "$PhenomenaWindow.ExportDoneText", MessageBoxButton.YesNo);
                if (answer == MessageBoxResult.Yes)
                {
                    Process.Start(file);
                }
            }
        }

        private void SelectAstroEvent(AstroEventVM ev)
        {
            OnEventSelected?.Invoke(new AstroEvent(ev.JulianDay, ev.Text, ev.PrimaryBody, ev.SecondaryBody));
        }
    }

    public interface IAstroEventsWriter
    {
        void Write(ICollection<AstroEvent> events, string file, double utcOffset);
    }

    public class AstroEventsTextWriter : IAstroEventsWriter
    {
        public void Write(ICollection<AstroEvent> events, string file, double utcOffset)
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

    public class AstroEventsGenericCsvWriter : IAstroEventsWriter
    {
        public void Write(ICollection<AstroEvent> events, string file, double utcOffset)
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

    public class AstroEventsGoogleCalendarCsvWriter : IAstroEventsWriter
    {
        public void Write(ICollection<AstroEvent> events, string file, double utcOffset)
        {
            using (var writer = File.CreateText(file))
            {
                // header
                writer.WriteLine($"\"Subject\",\"Start Date\",\"Start Time\",\"End Date\",\"End Time\",\"All Day Event\",\"Description\"");

                foreach (var e in events)
                {
                    DateTime date = new Date(e.JulianDay, utcOffset).ToDateTime();
                    string text = e.Text;
                    string startDate = date.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
                    string startTime = e.NoExactTime ? "" : date.ToString("hh:mm tt", CultureInfo.InvariantCulture);
                    string endDate = e.NoExactTime ? "" : startDate;
                    string endTime = e.NoExactTime ? "" : startTime;
                    string allDay = e.NoExactTime ? "True" : "False";
                    string[] columns = new string[] { text, startDate, startTime, endDate, endTime, allDay, text };
                    writer.WriteLine(string.Join(",", columns.Select(x => $"\"{x}\"")));
                }
                writer.Flush();
                writer.Close();
            }
        }
    }

    public class AstroEventsICalendarWriter : IAstroEventsWriter
    {
        public void Write(ICollection<AstroEvent> events, string file, double utcOffset)
        {
            using (var writer = File.CreateText(file))
            {
                string appVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
                string created = DateTime.UtcNow.ToString("yyyyMMddTHHmmss");

                writer.WriteLine("BEGIN:VCALENDAR");
                writer.WriteLine("VERSION:2.0");
                writer.WriteLine($"PRODID:-//Astrarium/Astrarium {appVersion}//EN");
                writer.WriteLine("X-WR-CALNAME:Astronomical phenomena");
                writer.WriteLine("X-WR-TIMEZONE:UTC");
                writer.WriteLine("X-WR-CALDESC:Astronomical phenomena");

                foreach (var e in events)
                {
                    DateTime localDate = new Date(e.JulianDay, utcOffset).ToDateTime();
                    string utcTimestamp = $"{TimeZoneInfo.ConvertTimeToUtc(localDate):yyyyMMddTHHmmss}Z";
                    string startDate = e.NoExactTime ? localDate.ToString("yyyyMMdd") : utcTimestamp;
                    string endDate = e.NoExactTime ? localDate.AddDays(1).ToString("yyyyMMdd") : utcTimestamp;
                    string uid = Guid.NewGuid().ToString();
                    writer.WriteLine("BEGIN:VEVENT");
                    writer.WriteLine($"DTSTAMP:{created}");
                    writer.WriteLine($"DTSTART:{startDate}");
                    writer.WriteLine($"DTEND:{endDate}");
                    writer.WriteLine($"UID:{uid}");
                    writer.WriteLine($"DESCRIPTION:{e.Text}");
                    writer.WriteLine($"SUMMARY:{e.Text}");
                    writer.WriteLine("END:VEVENT");
                }

                writer.WriteLine("END:VCALENDAR");

                writer.Flush();
                writer.Close();
            }
        }
    }
}