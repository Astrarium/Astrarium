using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Astrarium.Plugins.JupiterMoons
{
    public class JupiterMoonsVM : ViewModelBase
    {
        private readonly ISky sky;
        private readonly ISkyMap map;
        private readonly JupiterMoonsCalculator calculator = null;

        private readonly CelestialObject jupiter = null;
        private readonly CelestialObject[] moons = new CelestialObject[4];

        #region Formatters

        private readonly IEphemFormatter DateFormatter = Formatters.Date;
        private readonly IEphemFormatter TimeFormatter = Formatters.Time;
        private readonly IEphemFormatter DurationFormatter = new Formatters.TimeFormatter(withSeconds: true);
        private readonly IEphemFormatter AltitudeFormatter = new Formatters.SignedDoubleFormatter(1, "\u00B0");

        #endregion Formatters

        #region Commands

        public ICommand ShowEventBeginCommand => new Command<EventsTableItem>(ShowEventBegin);
        public ICommand ShowEventEndCommand => new Command<EventsTableItem>(ShowEventEnd);

        #endregion Commands

        public JupiterMoonsVM(ISky sky, ISkyMap  map)
        {
            this.sky = sky;
            this.map = map;
            this.calculator = new JupiterMoonsCalculator();

            jupiter = sky.Search("@Jupiter", obj => true).FirstOrDefault();
            for (int i = 0; i < 4; i++)
            {
                moons[i] = sky.Search($"@Jupiter-{i+1}", obj => true).FirstOrDefault();
            }

            Calculate();
        }

        private async void Calculate()
        {
            Date now = sky.Context.GetDate(sky.Context.JulianDay);
            await calculator.SetDate(now.Year, now.Month, sky.Context.GeoLocation);
            ICollection<JovianEvent> events = await calculator.GetEvents();

            foreach (var e in events.OrderBy(e => e.JdBegin))
            {
                string notes = null;

                if (e.IsEclipsedAtBegin)
                    notes = "Eclipsed by Jupiter at begin";
                if (e.IsOccultedAtBegin)
                    notes = "Occulted by Jupiter at begin";
                if (e.IsEclipsedAtEnd)
                    notes = "Eclipsed by Jupiter at end";
                if (e.IsOccultedAtEnd)
                    notes = "Occulted by Jupiter at end";

                var dateBegin = new Date(e.JdBegin, sky.Context.GeoLocation.UtcOffset);
                var dateEnd = new Date(e.JdEnd, sky.Context.GeoLocation.UtcOffset);

                EventsTable.Add(new EventsTableItem()
                {
                    JdBegin = e.JdBegin,
                    JdEnd = e.JdEnd,
                    MoonNumber = e.MoonNumber,
                    BeginDate = DateFormatter.Format(dateBegin),
                    BeginTime = TimeFormatter.Format(dateBegin),
                    EndTime = TimeFormatter.Format(dateEnd),
                    Duration = DurationFormatter.Format(e.JdEnd - e.JdBegin),
                    JupiterAltBegin = AltitudeFormatter.Format(e.JupiterAltBegin),
                    JupiterAltEnd = AltitudeFormatter.Format(e.JupiterAltEnd),
                    SunAltBegin = AltitudeFormatter.Format(e.SunAltBegin),
                    SunAltEnd = AltitudeFormatter.Format(e.SunAltEnd),
                    Code = e.Code,
                    Text = e.Text,
                    Notes = notes
                });
            }
        }

        private void ShowEventBegin(EventsTableItem e)
        {
            ShowEvent(e, e.JdBegin);
        }

        private void ShowEventEnd(EventsTableItem e)
        {
            ShowEvent(e, e.JdEnd);
        }

        private void ShowEvent(EventsTableItem e, double jd)
        {
            var body = e.MoonNumber > 0 ? moons[e.MoonNumber - 1] : jupiter;
            if (body != null)
            {
                sky.SetDate(jd);
                map.GoToObject(body, TimeSpan.Zero);
            }
        }

        public ObservableCollection<EventsTableItem> EventsTable
        {
            get => GetValue(nameof(EventsTable), new ObservableCollection<EventsTableItem>());
            set => SetValue(nameof(EventsTable), value);
        }
    }

    

    
}
