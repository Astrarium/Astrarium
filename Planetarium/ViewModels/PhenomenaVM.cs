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
    public class PhenomenaVM : ViewModelBase
    {
        public Command SaveToFileCommand { get; private set; }
        public Command CloseCommand { get; private set; }
        public Command<double> SetJulianDayCommand { get; private set; }
        public double JulianDay { get; private set; }
        public IEnumerable<IGrouping<string, AstroEventVM>> Events { get; private set; }

        private readonly IViewManager viewManager;
        private readonly Sky sky;
        
        public PhenomenaVM(IViewManager viewManager, Sky sky)
        {
            this.viewManager = viewManager;
            this.sky = sky;

            SaveToFileCommand = new Command(SaveToFile);
            CloseCommand = new Command(Close);
            SetJulianDayCommand = new Command<double>(SetJulianDay);
        }

        public void SetEvents(ICollection<AstroEvent> events)
        {
            Events = events.Select(e => new AstroEventVM() { JulianDay = e.JulianDay, Text = e.Text, NoExactTime = e.NoExactTime, Time = Formatters.TimeOnly.Format(new Date(e.JulianDay, sky.Context.GeoLocation.UtcOffset)) })
                .GroupBy(e => Formatters.DateOnly.Format(new Date(e.JulianDay, sky.Context.GeoLocation.UtcOffset)));
        }

        private void SaveToFile()
        {
            var result = viewManager.ShowSaveFileDialog("Save to file", "Phenomena", ".csv", "Text files (*.txt)|*.txt|Comma-separated files (*.csv)|*.csv");
            if (result != null)
            {
                
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
        public string Time { get; set; }
        public double JulianDay { get; set; }
        public string Text { get; set; }
        public bool NoExactTime { get; set; }
    }
}
