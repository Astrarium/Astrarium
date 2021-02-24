using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.ViewModels
{
    /// <summary>
    /// Defines ViewModel for the <see cref="Views.DateWindow"/> View. 
    /// </summary>
    public class DateVM : ViewModelBase
    {
        /// <summary>
        /// Called when user clicks on "Set Current Date&Time" link.
        /// </summary>
        public Command SetCurrentDateCommand => new Command(SetCurrentDate);

        /// <summary>
        /// Called when user selects date in the dialog.
        /// </summary>
        public Command SelectDateCommand => new Command(SelectDate);

        /// <summary>
        /// Called when month part decrements over minimal value.
        /// </summary>
        public Command LoopMonthDecrementCommand => new Command(LoopMonthDecrement);

        /// <summary>
        /// Called when month part increments over maximal value.
        /// </summary>
        public Command LoopMonthIncrementCommand => new Command(LoopMonthIncrement);

        /// <summary>
        /// Called when day part decrements over minimal value.
        /// </summary>
        public Command LoopDayDecrementCommand => new Command(LoopDayDecrement);

        /// <summary>
        /// Called when day part increments over maximal value.
        /// </summary>
        public Command LoopDayIncrementCommand => new Command(LoopDayIncrement);

        /// <summary>
        /// Called when hours hours decrements over minimal value.
        /// </summary>
        public Command LoopHoursDecrementCommand => new Command(LoopHoursDecrement);

        /// <summary>
        /// Called when hours part increments over maximal value.
        /// </summary>
        public Command LoopHoursIncrementCommand => new Command(LoopHoursIncrement);

        /// <summary>
        /// Called when minutes part decrements over minimal value.
        /// </summary>
        public Command LoopMinutesDecrementCommand => new Command(LoopMinutesDecrement);

        /// <summary>
        /// Called when minutes part increments over maximal value.
        /// </summary>
        public Command LoopMinutesIncrementCommand => new Command(LoopMinutesIncrement);

        /// <summary>
        /// Called when seconds part decrements over minimal value.
        /// </summary>
        public Command LoopSecondsDecrementCommand => new Command(LoopSecondsDecrement);

        /// <summary>
        /// Called when seconds part increments over maximal value.
        /// </summary>
        public Command LoopSecondsIncrementCommand => new Command(LoopSecondsIncrement);

        /// <summary>
        /// Display mode for the editor.
        /// </summary>
        public DateOptions DisplayMode { get; private set; } = DateOptions.DateTime;

        /// <summary>
        /// Gets UTC offset, in hours.
        /// </summary>
        public double UtcOffset { get; private set; }

        /// <summary>
        /// Gets array of abbreviated months names
        /// </summary>
        public string[] ShortMonthsNames { get; private set; }

        /// <summary>
        /// Gets array of full months names
        /// </summary>
        public string[] FullMonthsNames { get; private set; }

        /// <summary>
        /// Gets number of days in the specified month and year.
        /// </summary>
        public int DaysCount
        {
            get
            {
                return Date.DaysInMonth(_Year, _SelectedMonth + 1);
            }
        }

        /// <summary>
        /// Gets currently edited Julian Day value.
        /// </summary>
        public double JulianDay
        {
            get
            {
                Date date = new Date(Year, SelectedMonth + 1, Day + TimeSpan.FromHours(Hours).TotalDays + TimeSpan.FromMinutes(Minutes).TotalDays + TimeSpan.FromSeconds(Seconds).TotalDays);
                return Date.JulianDay(date, UtcOffset);
            }
            set
            {
                SetJulianDay(value);
            }
        }

        /// <summary>
        /// Backing field for <see cref="Year"/> property.
        /// </summary>
        private int _Year = DateTime.Now.Year;

        /// <summary>
        /// Get Year component of edited date.
        /// </summary>
        public int Year
        {
            get
            {
                return _Year;
            }
            set
            {
                _Year = Math.Max(-4000, Math.Min(value, 9999));
                NotifyPropertyChanged(nameof(DaysCount), nameof(Year), nameof(JulianDay));
            }
        }

        /// <summary>
        /// Backing field for <see cref="Day"/> property.
        /// </summary>
        private int _Day = DateTime.Now.Day;

        /// <summary>
        /// Get Day component of edited date.
        /// </summary>
        public int Day
        {
            get
            {
                return _Day;
            }
            set
            {
                _Day = Math.Max(1, Math.Min(value, DaysCount));
                NotifyPropertyChanged(nameof(Day), nameof(JulianDay));
            }
        }

        /// <summary>
        /// Backing field for <see cref="SelectedMonth"/> property.
        /// </summary>
        private int _SelectedMonth = DateTime.Now.Month - 1;

        /// <summary>
        /// Get selected index in the month combobox control
        /// </summary>
        public int SelectedMonth
        {
            get
            {
                return _SelectedMonth;
            }
            set
            {
                _SelectedMonth = Math.Max(0, Math.Min(value, 11));
                NotifyPropertyChanged(nameof(DaysCount), nameof(SelectedMonth), nameof(JulianDay));
            }
        }

        /// <summary>
        /// Backing field for <see cref="Hours"/> property.
        /// </summary>
        private int _Hours = DateTime.Now.Hour;

        /// <summary>
        /// Get Hours component of edited date.
        /// </summary>
        public int Hours
        {
            get
            {
                return _Hours;
            }
            set
            {
                _Hours = Math.Max(0, Math.Min(value, 23));
                NotifyPropertyChanged(nameof(Hours), nameof(JulianDay));
            }
        }

        /// <summary>
        /// Backing field for <see cref="Minutes"/> property.
        /// </summary>
        private int _Minutes = DateTime.Now.Minute;

        /// <summary>
        /// Get Minutes component of edited date.
        /// </summary>
        public int Minutes
        {
            get
            {
                return _Minutes;
            }
            set
            {
                _Minutes = Math.Max(0, Math.Min(value, 59));
                NotifyPropertyChanged(nameof(Minutes), nameof(JulianDay));
            }
        }

        /// <summary>
        /// Backing field for <see cref="Seconds"/> property.
        /// </summary>
        private int _Seconds = DateTime.Now.Second;

        /// <summary>
        /// Get Seconds component of edited date.
        /// </summary>
        public int Seconds
        {
            get
            {
                return _Seconds;
            }
            set
            {
                _Seconds = Math.Max(0, Math.Min(value, 59));
                NotifyPropertyChanged(nameof(Seconds), nameof(JulianDay));
            }
        }

        /// <summary>
        /// Command handler for <see cref="SetCurrentDateCommand"/>
        /// </summary>
        private void SetCurrentDate()
        {
            SetJulianDay(new Date(DateTime.Now).ToJulianEphemerisDay());
        }

        /// <summary>
        /// Command handler for <see cref="SelectDateCommand"/>
        /// </summary>
        private void SelectDate()
        {
            Close(true);
        }

        /// <summary>
        /// Command handler for <see cref="LoopMonthDecrementCommand"/>
        /// </summary>
        private void LoopMonthDecrement()
        {
            SetJulianDay(JulianDay - 31);
        }

        /// <summary>
        /// Command handler for <see cref="LoopMonthIncrementCommand"/>
        /// </summary>
        private void LoopMonthIncrement()
        {
            SetJulianDay(JulianDay + 31);
        }

        /// <summary>
        /// Command handler for <see cref="LoopDayDecrementCommand"/>
        /// </summary>
        private void LoopDayDecrement()
        {
            SetJulianDay(JulianDay - 1); 
        }

        /// <summary>
        /// Command handler for <see cref="LoopDayIncrementCommand"/>
        /// </summary>
        private void LoopDayIncrement()
        {
            SetJulianDay(JulianDay + 1);
        }

        /// <summary>
        /// Command handler for <see cref="LoopHoursDecrementCommand"/>
        /// </summary>
        private void LoopHoursDecrement()
        {
            SetJulianDay(JulianDay - TimeSpan.FromHours(1).TotalDays);
        }

        /// <summary>
        /// Command handler for <see cref="LoopHoursIncrementCommand"/>
        /// </summary>
        private void LoopHoursIncrement()
        {
            SetJulianDay(JulianDay + TimeSpan.FromHours(1).TotalDays);
        }

        /// <summary>
        /// Command handler for <see cref="LoopMinutesDecrementCommand"/>
        /// </summary>
        private void LoopMinutesDecrement()
        {
            SetJulianDay(JulianDay - TimeSpan.FromMinutes(1).TotalDays);
        }

        /// <summary>
        /// Command handler for <see cref="LoopMinutesIncrementCommand"/>
        /// </summary>
        private void LoopMinutesIncrement()
        {
            SetJulianDay(JulianDay + TimeSpan.FromMinutes(1).TotalDays);
        }

        /// <summary>
        /// Command handler for <see cref="LoopSecondsDecrementCommand"/>
        /// </summary>
        private void LoopSecondsDecrement()
        {
            SetJulianDay(JulianDay - TimeSpan.FromSeconds(1).TotalDays);
        }

        /// <summary>
        /// Command handler for <see cref="LoopSecondsIncrementCommand"/>
        /// </summary>
        private void LoopSecondsIncrement()
        {
            SetJulianDay(JulianDay + TimeSpan.FromSeconds(1.001).TotalDays);
        }

        /// <summary>
        /// Sets julian day value to editor controls.
        /// </summary>
        /// <param name="jd">Julian Day to be set.</param>
        private void SetJulianDay(double jd)
        {
            Date d = new Date(jd, UtcOffset);

            SelectedMonth = d.Month - 1;
            Day = (int)d.Day;
            Year = d.Year;
            Hours = d.Hour;
            Minutes = d.Minute;
            Seconds = d.Second;
        }

        /// <summary>
        /// Creates new instance of <see cref="DateVM"/>.
        /// </summary>
        /// <param name="jd">Julian Day to be set in the editor</param>
        /// <param name="utcOffset">UTC offset, in hours</param>
        /// <param name="displayMode">Editor display mode</param>
        public DateVM(double jd, double utcOffset, DateOptions displayMode = DateOptions.DateTime)
        {
            var dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
            ShortMonthsNames = dateTimeFormat.AbbreviatedMonthNames.Take(12).ToArray();
            FullMonthsNames = dateTimeFormat.MonthNames.Take(12).ToArray();

            UtcOffset = utcOffset;
            DisplayMode = displayMode;            

            SetJulianDay(jd);
        }
    }
}
