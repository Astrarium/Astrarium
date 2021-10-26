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
        private CelestialObject Moon { get; set; }

        #region Commands

        public ICommand ChangeYearCommand => new Command(ChangeYear);
        public ICommand PrevYearCommand => new Command(PrevYear);
        public ICommand NextYearCommand => new Command(NextYear);

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

        #endregion Bindable properties

        public MeteorShowersVM(MeteorsCalculator calc, ISky sky)
        {
            Meteors = calc.Meteors;
            Sky = sky;
            Year = sky.Context.GetDate(sky.Context.JulianDay).Year;
            Moon = Sky.Search("@moon", f => true).FirstOrDefault();
            Calculate();
        }

        private void ChangeYear()
        {

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

        private void Calculate()
        {
            if (Moon != null)
            {
                double from = Date.JulianDay0(Year) + 0.5;
                double to = from + (Date.IsLeapYear(Year) ? 366 : 365);
                MoonPhaseData = Sky.GetEphemerides(Moon, from, to, 1, new string[] { "Phase" })
                .Select(e => (float)e[0].GetValue<double>()).ToArray();
            }
        }
    }
}
