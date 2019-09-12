using ADK;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.ViewModels
{
    /// <summary>
    /// Defines ViewModel for the <see cref="Views.DateWindow"/> View. 
    /// </summary>
    public class DateVM : ViewModelBase
    {
        /// <summary>
        /// Called when user clicks on "Set Current Date&Time" link.
        /// </summary>
        public Command SetCurrentDateCommand { get; private set; }

        /// <summary>
        /// Called when user selects date in the dialog.
        /// </summary>
        public Command SelectDateCommand { get; private set; }

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
                _Year = value;
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
                _Day = value;
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
                _SelectedMonth = value;
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
                _Hours = value;
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
                _Minutes = value;
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
                _Seconds = value;
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
            SetCurrentDateCommand = new Command(SetCurrentDate);
            SelectDateCommand = new Command(SelectDate);

            var dateTimeFormat = CultureInfo.InvariantCulture.DateTimeFormat;
            ShortMonthsNames = dateTimeFormat.AbbreviatedMonthNames.Take(12).ToArray();
            FullMonthsNames = dateTimeFormat.MonthNames.Take(12).ToArray();

            UtcOffset = utcOffset;
            DisplayMode = displayMode;            

            SetJulianDay(jd);
        }
    }

    /// <summary>
    /// Options for displaying date values
    /// </summary>
    public enum DateOptions
    {
        DateTime,
        DateOnly,
        MonthYear
    }
}
