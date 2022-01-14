using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Astrarium.Plugins.Meteors
{
    public class MeteorShowersVM : ViewModelBase
    {
        private ISky Sky { get; set; }
        private MeteorsCalculator Calculator;
        private CelestialObject Moon { get; set; }

        #region Commands

        public ICommand ChangeYearCommand => new Command(ChangeYear);
        public ICommand PrevYearCommand => new Command(PrevYear);
        public ICommand NextYearCommand => new Command(NextYear);
        public ICommand ShowMeteorInfoCommand => new Command<Meteor>(ShowMeteorInfo);

        #endregion Commands

        #region Bindable properties

        public ICollection<Meteor> Meteors { get; set; }

        public ICollection<float> MoonPhaseData
        {
            get => GetValue<ICollection<float>>(nameof(MoonPhaseData));
            set => SetValue(nameof(MoonPhaseData), value);
        }

        public int Year
        {
            get => GetValue<int>(nameof(Year));
            set => SetValue(nameof(Year), value);
        }

        public double JulianDay0
        {
            get => GetValue<double>(nameof(JulianDay0));
            set => SetValue(nameof(JulianDay0), value);
        }

        public double JulianDay
        {
            get => GetValue<double>(nameof(JulianDay));
            set
            {
                SetValue(nameof(JulianDay), value);
                if (value > 0)
                {
                    var date = Sky.Context.GetDate(value);
                    DateString = Text.Get("MeteorShowersView.StatusBar.Date", ("Date", Formatters.Date.Format(date)));
                    var doy = Date.DayOfYear(date) - 1;
                    if (doy >= 0 && doy < MoonPhaseData.Count)
                    {
                        MoonPhaseString = Text.Get("MeteorShowersView.StatusBar.LunarPhase", ("Phase", Formatters.Phase.Format(MoonPhaseData.ElementAt(doy))));
                    }
                    else
                    {
                        MoonPhaseString = null;
                    }

                    IsMouseOver = true;
                    ActiveCountString = Text.Get("MeteorShowersView.StatusBar.ActiveShowersCount", ("Count", Meteors.Count(m => m.Begin <= doy && doy <= m.End).ToString()));
                }
                else
                {
                    IsMouseOver = false;
                    DateString = null;
                    MoonPhaseString = null;
                    ActiveCountString = null;
                }
            }
        }

        public string DateString
        {
            get => GetValue<string>(nameof(DateString));
            set => SetValue(nameof(DateString), value);
        }

        public string MoonPhaseString
        { 
            get => GetValue<string>(nameof(MoonPhaseString));
            set => SetValue(nameof(MoonPhaseString), value);
        }

        public string ActiveCountString
        {
            get => GetValue<string>(nameof(ActiveCountString));
            set => SetValue(nameof(ActiveCountString), value);
        }

        public bool IsMouseOver
        {
            get => GetValue<bool>(nameof(IsMouseOver));
            set => SetValue(nameof(IsMouseOver), value);
        }

        #endregion Bindable properties

        public MeteorShowersVM(MeteorsCalculator calc, ISky sky)
        {
            Meteors = calc.Meteors.OrderBy(x => x.Max).ToArray();
            Sky = sky;
            Calculator = calc;
            Year = sky.Context.GetDate(sky.Context.JulianDay).Year;
            Moon = Sky.Search("@moon", f => true).FirstOrDefault();
            Calculate();
        }

        private void ChangeYear()
        {
            double? jd = ViewManager.ShowDateDialog(Date.JulianEphemerisDay(new Date(Year, 1, 1)), 0, DateOptions.Year);
            if (jd != null)
            {
                Year = new Date(jd.Value, 0).Year;
                Calculate();
            }
        }

        private void PrevYear()
        {
            Year--;
            Calculate();
        }

        private void NextYear()
        {
            Year++;
            Calculate();
        }

        private void ShowMeteorInfo(Meteor m)
        {
            SkyContext c = new SkyContext(JulianDay, Sky.Context.GeoLocation);
            int year = c.GetDate(c.JulianDay).Year;
            var offset = c.GeoLocation.UtcOffset;
            var jd0 = Date.DeltaT(c.JulianDay) / 86400.0 + Date.JulianDay0(year) - offset / 24;
            var begin = new Date(jd0 + m.Begin, offset);
            var max = new Date(jd0 + m.Max, offset);
            var end = new Date(jd0 + m.End, offset);
            SkyContext cMax = new SkyContext(jd0 + m.Max, c.GeoLocation, c.PreferFastCalculation);
            var phase = Calculator.LunarPhaseAtMax(cMax, m);

            var sb = new StringBuilder();
            sb.AppendLine($"**{Text.Get("MeteorShowersInfoDialog.Names")}**  ");
            sb.AppendLine(string.Join(", ", m.Names));
            sb.AppendLine();

            sb.AppendLine($"**{Text.Get("Meteor.Activity.Begin")}**  ");
            sb.AppendLine(Formatters.Date.Format(begin));
            sb.AppendLine();

            sb.AppendLine($"**{Text.Get("Meteor.Activity.Max")}**  ");
            sb.AppendLine(Formatters.Date.Format(max));
            sb.AppendLine();

            sb.AppendLine($"**{Text.Get("Meteor.Activity.End")}**  ");
            sb.AppendLine(Formatters.Date.Format(end));
            sb.AppendLine();

            sb.AppendLine($"**{Text.Get("Meteor.Data.ZHR")}**  ");
            sb.AppendLine(m.ZHR);
            sb.AppendLine();

            sb.AppendLine($"**{Text.Get("Meteor.Data.ActivityClass")}**  ");
            sb.AppendLine(m.ActivityClass.ToString());
            sb.AppendLine();

            sb.AppendLine($"**{Text.Get("Meteor.Activity.LunarPhaseAtMax")}**  ");
            sb.AppendLine(Formatters.Phase.Format(phase));
            sb.AppendLine();

            ViewManager.ShowMessageBox("$MeteorShowersInfoDialog.Title", sb.ToString());
        }

        private void Calculate()
        {
            if (Moon != null)
            {
                double from = Date.JulianDay0(Year) + 0.5;
                double to = from + (Date.IsLeapYear(Year) ? 366 : 365);
                MoonPhaseData = Sky.GetEphemerides(Moon, from, to, 1, new string[] { "Phase" })
                    .Select(e => (float)e[0].GetValue<double>()).ToArray();
                JulianDay0 = Date.JulianDay0(Year);
            }
        }
    }
}
