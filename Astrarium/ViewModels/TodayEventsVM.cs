using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Astrarium.ViewModels
{
    public class TodayEventsVM : ViewModelBase
    {
        private readonly ISky sky;
        private double julianDay;
        public string Date { get; private set; }

        public Command PrevDayCommand { get; private set; }
        public Command NextDayCommand { get; private set; }
        public Command ChooseDateCommand { get; private set; }
        public Command CloseCommand { get; private set; }
        public Command<AstroEventVM> SelectAstroEventCommand { get; private set; }
        public event Action<AstroEvent> OnEventSelected;

        public ICollection<AstroEventVM> Events { get; private set; }

        public TodayEventsVM(ISky sky)
        {
            this.sky = sky;
            this.julianDay = sky.Context.JulianDay;
            PrevDayCommand = new Command(PrevDay);
            NextDayCommand = new Command(NextDay);
            CloseCommand = new Command(Close);
            ChooseDateCommand = new Command(ChooseDate);
            SelectAstroEventCommand = new Command<AstroEventVM>(SelectAstroEvent);
            CalculateEvents();
        }

        private void PrevDay()
        {
            julianDay--;
            CalculateEvents();
        }

        private void NextDay()
        {
            julianDay++;
            CalculateEvents();
        }

        private void ChooseDate()
        {
            double? jd = ViewManager.ShowDateDialog(julianDay, sky.Context.GeoLocation.UtcOffset, displayMode: DateOptions.DateOnly);
            if (jd.HasValue)
            {
                julianDay = jd.Value;
                CalculateEvents();
            }
        }

        private async void CalculateEvents()
        {
            Date date = new Date(julianDay, sky.Context.GeoLocation.UtcOffset);
            var jdMidnight = julianDay - (date.Day - Math.Truncate(date.Day));

            var events = await Task.Run(() => sky.GetEvents(
                jdMidnight,
                jdMidnight + 1,
                sky.GetEventsCategories().ToArray()));

            Events = events
                .Select(e => new AstroEventVM(e, sky.Context.GeoLocation.UtcOffset))
                .ToArray();

            Date = Formatters.Date.Format(new Date(jdMidnight, sky.Context.GeoLocation.UtcOffset));

            NotifyPropertyChanged(nameof(Events), nameof(Date));
        }

        private void SelectAstroEvent(AstroEventVM ev)
        {
            OnEventSelected?.Invoke(new AstroEvent(ev.JulianDay, ev.Text, ev.PrimaryBody, ev.SecondaryBody));
        }
    }
}
