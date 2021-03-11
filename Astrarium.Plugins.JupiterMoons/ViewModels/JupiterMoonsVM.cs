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
using System.Text.RegularExpressions;
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
        private ICollection<JovianEvent> events = null;

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

        #region Bindable properties

        public bool FilterBodyIo
        {
            get => GetValue(nameof(FilterBodyIo), true);
            set { SetValue(nameof(FilterBodyIo), value); ApplyFilter(); }
        }

        public bool FilterBodyEuropa
        {
            get => GetValue(nameof(FilterBodyEuropa), true);
            set { SetValue(nameof(FilterBodyEuropa), value); ApplyFilter(); }
        }

        public bool FilterBodyGanymede
        {
            get => GetValue(nameof(FilterBodyGanymede), true);
            set { SetValue(nameof(FilterBodyGanymede), value); ApplyFilter(); }
        }

        public bool FilterBodyCallisto
        {
            get => GetValue(nameof(FilterBodyCallisto), true);
            set { SetValue(nameof(FilterBodyCallisto), value); ApplyFilter(); }
        }

        public bool FilterTransits
        {
            get => GetValue(nameof(FilterTransits), true);
            set { SetValue(nameof(FilterTransits), value); ApplyFilter(); }
        }

        public bool FilterShadowTransits
        {
            get => GetValue(nameof(FilterShadowTransits), true);
            set { SetValue(nameof(FilterShadowTransits), value); ApplyFilter(); }
        }

        public bool FilterEclipses
        {
            get => GetValue(nameof(FilterEclipses), true);
            set { SetValue(nameof(FilterEclipses), value); ApplyFilter(); }
        }

        public bool FilterOccultations
        {
            get => GetValue(nameof(FilterOccultations), true);
            set { SetValue(nameof(FilterOccultations), value); ApplyFilter(); }
        }

        public bool FilterMutualEclipses
        {
            get => GetValue(nameof(FilterMutualEclipses), true);
            set { SetValue(nameof(FilterMutualEclipses), value); ApplyFilter(); }
        }

        public bool FilterMutualOccultations
        {
            get => GetValue(nameof(FilterMutualOccultations), true);
            set { SetValue(nameof(FilterMutualOccultations), value); ApplyFilter(); }
        }

        public bool FilterJupiterAboveHorizon
        {
            get => GetValue(nameof(FilterJupiterAboveHorizon), false);
            set { SetValue(nameof(FilterJupiterAboveHorizon), value); ApplyFilter(); }
        }

        public bool FilterSunBelowHorizon
        {
            get => GetValue(nameof(FilterSunBelowHorizon), false);
            set { SetValue(nameof(FilterSunBelowHorizon), value); ApplyFilter(); }
        }

        #endregion Bindable properties

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
            events = await calculator.GetEvents();

            ApplyFilter();
        }

        private bool IsMatchFilter(JovianEvent e, bool filter, string code)
        {
            return !filter && Regex.IsMatch(e.Code, code) ? false : true;
        }

        private async void ApplyFilter()
        {
            await Task.Run(() =>
            {
                var items = new List<EventsTableItem>();

                foreach (var e in events.OrderBy(e => e.JdBegin)
                    .Where(e => IsMatchFilter(e, FilterBodyIo, "^.+1$"))
                    .Where(e => IsMatchFilter(e, FilterBodyEuropa, "^.+2$"))
                    .Where(e => IsMatchFilter(e, FilterBodyGanymede, "^.+3$"))
                    .Where(e => IsMatchFilter(e, FilterBodyCallisto, "^.+4$"))
                    .Where(e => IsMatchFilter(e, FilterTransits, "^T\\d$"))
                    .Where(e => IsMatchFilter(e, FilterShadowTransits, "^S\\d$"))
                    .Where(e => IsMatchFilter(e, FilterEclipses, "^E\\d$"))
                    .Where(e => IsMatchFilter(e, FilterOccultations, "^O\\d$"))
                    .Where(e => IsMatchFilter(e, FilterMutualEclipses, "^\\dE\\d$"))
                    .Where(e => IsMatchFilter(e, FilterMutualOccultations, "^\\dO\\d$"))
                    .Where(e => FilterJupiterAboveHorizon ? e.JupiterAltBegin > 0 || e.JupiterAltEnd > 0 : true)
                    .Where(e => FilterSunBelowHorizon ? e.SunAltBegin < 0 || e.SunAltEnd < 0 : true))
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

                    items.Add(new EventsTableItem()
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

                EventsTable = items.ToArray();
            });
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

        public EventsTableItem[] EventsTable
        {
            get => GetValue(nameof(EventsTable), new EventsTableItem[0]);
            set => SetValue(nameof(EventsTable), value);
        }
    }

}
