using System;
using System.Globalization;

namespace ADK
{
    /// <summary>
    /// Represents astronomical date.
    /// Contains methods and functions related to time, dates, calendars.
    /// </summary>
    public class Date
    {
        #region Epochs

        /// <summary>
        /// Epoch B1875.0 in julian days.
        /// </summary>
        public const double EPOCH_B1875 = 2405889.25855;

        /// <summary>
        /// Epoch B1900.0 in julian days.
        /// </summary>
        public const double EPOCH_B1900 = 2415020.3135;

        /// <summary>
        /// Epoch B1950.0 in julian days.
        /// </summary>
        public const double EPOCH_B1950 = 2433282.4235;

        /// <summary>
        /// Epoch J1900.0 in julian days.
        /// </summary>
        public const double EPOCH_J1900 = 2415020.0;

        /// <summary>
        /// Epoch J1950.0 in julian days.
        /// </summary>
        public const double EPOCH_J1950 = 2433282.5;

        /// <summary>
        /// Epoch J1975.0 in julian days.
        /// </summary>
        public const double EPOCH_J1975 = 2442413.75;

        /// <summary>
        /// Epoch J2000.0 in julian days.
        /// </summary>
        public const double EPOCH_J2000 = 2451545.0;

        /// <summary>
        /// Epoch J2050.0 in julian days.
        /// </summary>
        public const double EPOCH_J2050 = 2469807.5;

        #endregion Epochs

        /// <summary>
        /// Year. Positive value means A.D. year. Zero value means 1 B.C. -1 value = 2 B.C., -2 = 3 B.C. etc.
        /// </summary>
        public int Year { get; private set; }

        /// <summary>
        /// Month. 1 = January, 2 = February, etc.
        /// </summary>
        public int Month { get; private set; }

        /// <summary>
        /// Day of month with fractions (if needed). For example, 17.5 means 17th day of month, 12:00.
        /// </summary>
        public double Day { get; private set; }

        /// <summary>
        /// Hour of day. From 0 to 23.
        /// </summary>
        public int Hour
        {
            get
            {
                double value = Day;
                value = (value - (int)value) * 24.0;
                return (int)value;
            }
        }

        /// <summary>
        /// Minutes. From 0 to 59.
        /// </summary>
        public int Minute
        {
            get
            {
                double value = Day;
                value = (value - (int)value) * 24.0;
                value = (value - (int)value) * 60.0;
                return (int)value;
            }
        }

        /// <summary>
        /// Seconds. From 0 to 59.
        /// </summary>
        public int Second
        {
            get
            {
                double value = Day;
                value = (value - (int)value) * 24.0;
                value = (value - (int)value) * 60.0;
                value = (value - (int)value) * 60.0;
                return (int)value;
            }
        }

        #region Constructors

        /// <summary>
        /// Creates new <see cref="Date"/> by year, month and day values.
        /// </summary>
        /// <param name="year">Year. Positive value means A.D. year. Zero value means 1 B.C. -1 value = 2 B.C., -2 = 3 B.C. etc.</param>
        /// <param name="month">Month. 1 = January, 2 = February, etc.</param>
        /// <param name="day">Day of month with fractions (if needed). For example, 17.5 means 17th day of month, 12:00.</param>
        public Date(int year, int month, double day)
        {
            if (month < 1 || month > 12)
                throw new ArgumentException("Month value should be from 1 to 12.", nameof(month));

            if (day < 0)
                throw new ArgumentException("Day value should be positive.", nameof(day));

            Year = year;
            Month = month;
            Day = day;
        }

        /// <summary>
        /// Creates new <see cref="Date"/> from <see cref="DateTime"/> object.
        /// </summary>
        /// <param name="dateTime"><see cref="DateTime"/> object to create a <see cref="Date"/> from.</param>
        public Date(DateTime dateTime)
        {
            DateTime uDateTime = dateTime.ToUniversalTime();
            Year = uDateTime.Year;
            Month = uDateTime.Month;
            Day = uDateTime.Day + uDateTime.TimeOfDay.TotalHours / 24.0;
        }

        /// <summary>
        /// Creates new <see cref="Date"/> instance from Julian Day value.
        /// </summary>
        /// <param name="jd">Julian Day</param>
        /// <remarks>
        /// Implementation is taken from AA2, ch. 7, p. 63.
        /// </remarks>
        public Date(double jd)
        {
            if (jd < 0)
                throw new ArgumentException("Julian Day value should be greater or equal to zero.", nameof(jd));

            jd += 0.5;
            int Z = (int)jd;
            double F = jd - Z;
            int A = 0;
            if (Z < 2299161)
            {
                A = Z;
            }
            else
            {
                int a = (int)((Z - 1867216.25) / 36524.25);
                A = Z + 1 + a - (int)(a / 4.0);
            }
            int B = A + 1524;
            int C = (int)((B - 122.1) / 365.25);
            int D = (int)(365.25 * C);
            int E = (int)((B - D) / 30.6001);
            Day = B - D - (int)(30.6001 * E) + F;
            if (E < 14) Month = E - 1;
            if (E == 14 || E == 15) Month = E - 13;
            if (Month > 2) Year = C - 4716;
            if (Month <= 2) Year = C - 4715;
        }

        /// <summary>
        /// Creates new date from julian day and UTC offset in hours
        /// </summary>
        /// <param name="jd">Julian Day to convert to local date</param>
        /// <param name="utcOffset">UTC offset in hours</param>
        public Date(double jd, double utcOffset) :
            this(jd + utcOffset / 24.0 - DeltaT(jd) / 86400)
        { }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Checks <see cref="Date"/> for equality
        /// </summary>
        /// <param name="obj">Object to compare with</param>
        /// <returns>
        /// True if two dates are equal, false otherwise.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj is Date)
            {
                Date other = obj as Date;
                return
                    other.Year == Year &&
                    other.Month == Month &&
                    Math.Abs(other.Day - Day) < 1e-9;
            }
            else
                return false;
        }

        /// <summary>
        /// Gets hash code for the <see cref="Date"/> object.
        /// </summary>
        /// <returns>Hash code for the <see cref="Date"/> object.</returns>
        public override int GetHashCode()
        {
            unchecked 
            {
                int hash = 17;
                hash = hash * 23 + Year.GetHashCode();
                hash = hash * 23 + Month.GetHashCode();
                hash = hash * 23 + Day.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Gets string that represents the <see cref="Date"/> object.
        /// </summary>
        /// <returns>
        /// String that represents the <see cref="Date"/> object.
        /// </returns>
        public override string ToString()
        {
            var culture = CultureInfo.InvariantCulture;
            string month = culture.DateTimeFormat.GetMonthName(Month);
            string day = Day.ToString("0.##", culture.NumberFormat);
            return string.Format($"{Year} {month} {day}");
        }

        #endregion Overrides

        #region Operators

        /// <summary>
        /// Gets difference between dates, in days.
        /// </summary>
        /// <returns>Difference between dates, in days.</returns>
        public static double operator -(Date left, Date right)
        {
            return left.ToJulianDay() - right.ToJulianDay();
        }

        /// <summary>
        /// Subsracts specified amount of days (with fractions if required) from the <see cref="Date"/>.
        /// </summary>
        /// <returns>New <see cref="Date"/> instance</returns>
        public static Date operator -(Date left, double right)
        {
            return new Date(left.ToJulianDay() - right);
        }

        /// <summary>
        /// Adds specified amount of days to the <see cref="Date"/>.
        /// </summary>
        /// <returns>>New <see cref="Date"/> instance</returns>
        public static Date operator +(Date left, double right)
        {
            return new Date(left.ToJulianDay() + right);
        }

        #endregion Operators

        #region Instance Methods

        /// <summary>
        /// Converts the <see cref="Date"/> to Julian Day value.
        /// </summary>
        /// <returns>Julian Day value.</returns>
        public double ToJulianDay()
        {
            return JulianDay(this);
        }

        /// <summary>
        /// Julian Ephemeris Day (JDE) for the <see cref="Date"/>.
        /// </summary>
        /// <returns>Julian Ephemeris Day (JDE) value.</returns>
        public double ToJulianEphemerisDay()
        {
            return JulianEphemerisDay(this);
        }

        /// <summary>
        /// Checks if the date's year is a leap (bissextile) year.
        /// </summary>
        /// <returns>True if the date's year is a leap (bissextile) year, false otherwise.</returns>
        public bool IsLeapYear()
        {
            return IsLeapYear(Year);
        }

        /// <summary>
        /// Gets day of the week for the <see cref="Date"/>.
        /// </summary>
        /// <returns><see cref="System.DayOfWeek"/> value.</returns>
        public DayOfWeek DayOfWeek()
        {
            return DayOfWeek(ToJulianDay());
        }

        /// <summary>
        /// Converts the date in Julian Calendar to a date in Gregorian Calendar. 
        /// </summary>
        /// <returns>
        /// Date in Gregorian Calendar
        /// </returns>
        public Date ToGregorianCalendarDate()
        {
            return JulianToGregorian(this);
        }

        /// <summary>
        /// Converts the date in Gregorian Calendar to a date in Julian Calendar. 
        /// </summary>
        /// <returns>
        /// Date in Julian Calendar
        /// </returns>
        public Date ToJulianCalendarDate()
        {
            return GregorianToJulian(this);
        }

        /// <summary>
        /// Calculates time difference between Dynamical and Universal Times (ΔT = TD - UT) for the date.
        /// </summary>
        /// <returns>
        /// The time difference expressed in seconds of time.
        /// </returns>
        public double DeltaT()
        {
            return DeltaT(this);
        }

        /// <summary>
        /// Gets mean sidereal time at Greenwich for given instant.
        /// </summary>
        /// <returns>Mean sidereal time at Greenwich, expressed in degrees.</returns>
        public double MeanSiderealTime()
        {
            return MeanSiderealTime(ToJulianDay());
        }

        #endregion Instance Methods

        #region Static methods

        /// <summary>
        /// Gets current <see cref="Date"/>.
        /// </summary>
        public static Date Now
        {
            get
            {
                return new Date(DateTime.Now);
            }
        }

        /// <summary>
        /// Converts the specified <see cref="Date"/> to Julian Day value.
        /// </summary>
        /// <returns>Julian Day value.</returns>
        public static double JulianDay(Date date)
        {
            return JulianDay(date.Year, date.Month, date.Day);
        }

        /// <summary>
        /// Converts the specified date to Julian Day value.
        /// </summary>
        /// <param name="Y">Year. Positive value means A.D. year. Zero value means 1 B.C. -1 value = 2 B.C., -2 = 3 B.C. etc.</param>
        /// <param name="M">Month value. 1 = January, 2 = February etc.</param>
        /// <param name="D">Day of month</param>
        /// <param name="h">Hour of day</param>
        /// <param name="m">Minutes</param>
        /// <param name="s">Seconds</param>
        /// <returns>Julian Day value.</returns>
        public static double JulianDay(int Y, int M, int D, int h, int m, int s)
        {
            double day = D + h / 24.0 + m / 1440.0 + s / 86400.0;
            return JulianDay(Y, M, day);
        }

        /// <summary>
        /// Converts the specified date to Julian Day value.
        /// </summary>
        /// <param name="Y">Year. Positive value means A.D. year. Zero value means 1 B.C. -1 value = 2 B.C., -2 = 3 B.C. etc.</param>
        /// <param name="M">Month value. 1 = January, 2 = February etc.</param>
        /// <param name="D">Day of month, with fractions.</param>
        /// <returns>Julian Day value.</returns>
        public static double JulianDay(int Y, int M, double D)
        { 
            bool isJulianCalendar = Y < 1582;

            if (Y == 1582)
            {
                if (M < 10)
                {
                    isJulianCalendar = true;
                }
                if (M == 10)
                {
                    if (D < 15)
                    {
                        isJulianCalendar = true;
                    }
                }
            }

            if (M <= 2)
            {
                M += 12;
                Y--;
            }

            int A = 0;
            int B = 0;

            if (!isJulianCalendar)
            {
                A = (int)(Y / 100.0);
                B = 2 - A + (int)(A / 4.0);
            }

            return (int)(365.25 * (Y + 4716)) + (int)(30.600001 * (M + 1)) + D + B - 1524.5;
        }

        /// <summary>
        /// Gets Julian day corresponding to January 0.0 of a given year. 
        /// </summary>
        /// <param name="year">Year in Gregorian calendar.</param>
        /// <returns>Julian day corresponding to January 0.0 of a given year.</returns>
        public static double JulianDay0(int year)
        {
            if (year < 1582)
                throw new ArgumentException("Year should be in Gregorian calendar (greater or equal to 1582).", nameof(year));

            int Y = year - 1;
            int A = Y / 100;
            return (int)(365.25 * Y) - A + (A / 4) + 1721424.5;
        }

        /// <summary>
        /// Gets Modified Julian Day (MJD) for a specified Julian Day.
        /// </summary>
        /// <param name="jd">Julian Day value</param>
        /// <returns>Modified Julian Day (MJD) value.</returns>
        public static double ModifiedJulianDay(double jd)
        {
            return jd - 2400000.5;
        }

        /// <summary>
        /// Checks if the date's year is a leap (bissextile) year.
        /// </summary>
        /// <param name="year">Date to check for a leap year.</param>
        /// <returns>True if the date's year is a leap (bissextile) year, false otherwise.</returns>
        public static bool IsLeapYear(int year)
        {
            if (year < 1582)
            {
                if (year % 4 == 0) return true;
            }
            if (year % 100 == 0 && (year / 100) % 4 != 0) return false;
            if (year % 4 == 0) return true;
            return false;
        }

        /// <summary>
        /// Gets day of the week for the specified Julian Day.
        /// </summary>
        /// <returns><see cref="System.DayOfWeek"/> value.</returns>
        /// <param name="jd">Julian Day to get the day of the week for.</param>
        /// <returns><see cref="System.DayOfWeek"/> value.</returns>
        public static DayOfWeek DayOfWeek(double jd)
        {
            double d = jd + 1.5;
            return (DayOfWeek)((int)d % 7);
        }

        /// <summary>
        /// Gets ordinal number of day in a year.
        /// </summary>
        /// <param name="date">Date to get ordinal number of day in a year.</param>
        /// <returns>Ordinal number of day in a year.</returns>
        public static int DayOfYear(Date date)
        {
            int K = IsLeapYear(date.Year) ? 1 : 2;
            return (int)((275 * date.Month / 9) - K * ((date.Month + 9) / 12) + date.Day - 30);
        }

        /// <summary>
        /// Number of days by months for regular (non-leap) year.
        /// </summary>
        private static readonly int[] DAYS_IN_MONTH = new int[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        /// <summary>
        /// Gets number of days in the specified month.
        /// </summary>
        /// <param name="year">Year</param>
        /// <param name="month">Month. 1 = Janaury, 2 = February etc.</param>
        /// <returns>Number of days for specified month and year.</returns>
        public static int DaysInMonth(int year, int month)
        {
            if (month < 1 || month > 12)
                throw new ArgumentException("Month value should be from 1 to 12.", nameof(month));

            return DAYS_IN_MONTH[month - 1] + (month == 2 ? (IsLeapYear(year) ? 1 : 0) : 0);
        }

        /// <summary>
        /// Gets <see cref="Date"/> of Gregorian Easter for a given year.
        /// The function valid for all years in the Gregorian calendar, hence from the year 1583 on. 
        /// </summary>
        /// <param name="year">Given year to find the date of the Easter Sunday.</param>
        /// <returns><see cref="Date"/> of Gregorian Easter.</returns>
        /// <remarks>
        /// Method is taken from AA2 book, ch.8 
        /// </remarks>
        public static Date GregorianEaster(int year)
        {
            if (year < 1582)
                throw new ArgumentException("Year should be in Gregorian calendar (greater or equal to 1582).", nameof(year));

            int x = year;
            int a = year % 19;
            int b = year / 100;
            int c = year % 100;
            int d = b / 4;
            int e = b % 4;
            int f = (b + 8) / 25;
            int g = (b - f + 1) / 3;
            int h = (19 * a + b - d - g + 15) % 30;
            int i = c / 4;
            int k = c % 4;
            int L = (32 + 2 * e + 2 * i - h - k) % 7;
            int m = (a + 11 * h + 22 * L) / 451;
            int z = h + L - 7 * m + 114;
            int n = z / 31;
            int p = z % 31;
            return new Date(year, n, p + 1);
        }

        /// <summary>
        /// Gets <see cref="Date"/> of Julian Easter for a given year.
        /// </summary>
        /// <param name="year">Given year to find the date of the Easter Sunday.</param>
        /// <returns><see cref="Date"/> of Julian Easter.</returns>
        /// <remarks>
        /// Method is taken from AA2 book, ch.8 
        /// </remarks>
        public static Date JulianEaster(int year)
        {
            int x = year;
            int a = x % 4;
            int b = x % 7;
            int c = x % 19;
            int d = (19 * c + 15) % 30;
            int e = (2 * a + 4 * b - d + 34) % 7;
            int z = d + e + 114;
            int f = z / 31;
            int g = z % 31;
            return new Date(year, f, g + 1);
        }

        /// <summary>
        /// Converts the date in Gregorian Calendar to a date in Julian Calendar. 
        /// </summary>
        /// <param name="date">Date in Gregorian Calendar</param>
        /// <returns>
        /// Date in Julian Calendar
        /// </returns>
        public static Date GregorianToJulian(Date date)
        {
            DateTime dt = new DateTime(date.Year, date.Month, (int)date.Day, new GregorianCalendar());
            JulianCalendar julianCalendar = new JulianCalendar();
            return new Date(
                julianCalendar.GetYear(dt), 
                julianCalendar.GetMonth(dt), 
                julianCalendar.GetDayOfMonth(dt));
        }

        /// <summary>
        /// Converts the date in Julian Calendar to a date in Gregorian Calendar. 
        /// </summary>
        /// <param name="date">Date in Julian Calendar</param>
        /// <returns>
        /// Date in Gregorian Calendar
        /// </returns>
        public static Date JulianToGregorian(Date date)
        {
            DateTime dt = new DateTime(date.Year, date.Month, (int)date.Day, new JulianCalendar());
            GregorianCalendar gregorianCalendar = new GregorianCalendar();
            return new Date(
                gregorianCalendar.GetYear(dt),
                gregorianCalendar.GetMonth(dt),
                gregorianCalendar.GetDayOfMonth(dt));
        }

        /// <summary>
        /// Calculates Julian Ephemeris Day (JDE) for the given <see cref="Date"/>.
        /// </summary>
        /// <param name="date">Date to calculate Julian Ephemeris Day (JDE)</param>
        /// <returns>Julian Ephemeris Day (JDE) value</returns>
        public static double JulianEphemerisDay(Date date)
        {
            double jd = JulianDay(date);
            return jd + DeltaT(date) / 86400.0;
        }

        /// <summary>
        /// Gets Julian Day corresponding to Besselian epoch of a specified year,
        /// for example: BesselianEpoch(1950) = B1950.0 = 2433282.4235
        /// </summary>
        /// <param name="year">Year of the epoch</param>
        /// <returns>Epoch value, in Julian Days</returns>
        public static double BesselianEpoch(int year)
        {
            return (year - 1900.0) * 365.242198781 + 2415020.31352;
        }

        /// <summary>
        /// Gets Julian Day corresponding to Julian epoch of a specified year,
        /// for example: JulianEpoch(2000) = J2000.0 = 2451545.00
        /// </summary>
        /// <param name="year">Year of the epoch</param>
        /// <returns>Epoch value, in Julian Days</returns>
        public static double JulianEpoch(int year)
        {
            return (year - 2000.0) * 365.25 + 2451545.0;
        }

        /// <summary>
        /// Calculates time difference between Dynamical and Universal Times (ΔT = TD - UT) for a given Julian Day.
        /// </summary>
        /// <param name="jd">julian Day for which the time difference should be calculated</param>
        /// <returns>The time difference expressed in seconds of time.</returns>
        /// <remarks>
        /// The polynomial expressions are taken from https://eclipse.gsfc.nasa.gov/SEhelp/deltatpoly2004.html
        /// </remarks>
        public static double DeltaT(double jd)
        {
            return DeltaT(new Date(jd));
        }

        /// <summary>
        /// Calculates time difference between Dynamical and Universal Times (ΔT = TD - UT) for a given date.
        /// </summary>
        /// <param name="date">Date for which the time difference should be calculated</param>
        /// <returns>The time difference expressed in seconds of time.</returns>
        /// <remarks>
        /// The polynomial expressions are taken from https://eclipse.gsfc.nasa.gov/SEhelp/deltatpoly2004.html
        /// </remarks>
        public static double DeltaT(Date date)
        {
            double y = date.Year + (date.Month - 0.5) / 12.0;
            double u = 0, u2 = 0, u3 = 0, u4 = 0, u5 = 0, u6 = 0, u7 = 0;
            double deltaT = 0;

            Action<double> powersOfU = (value) => {
                u = value;
                u2 = u * u;
                u3 = u2 * u;
                u4 = u3 * u;
                u5 = u4 * u;
                u6 = u5 * u;
                u7 = u6 * u;
            }; 

            if (date.Year < -500)
            {
                u = (y - 1820) / 100;
                deltaT = -20 + 32 * u * u;
            }
            else if (date.Year >= -500 && date.Year <= 500)
            {
                powersOfU(y / 100);
                deltaT = 10583.6 - 1014.41 * u + 33.78311 * u2 - 5.952053 * u3
                    - 0.1798452 * u4 + 0.022174192 * u5 + 0.0090316521 * u6;
            }
            else if (date.Year > 500 && date.Year <= 1600)
            {
                powersOfU((y - 1000) / 100);
                deltaT = 1574.2 - 556.01 * u + 71.23472 * u2 + 0.319781 * u3
                    - 0.8503463 * u4 - 0.005050998 * u5 + 0.0083572073 * u6;
            }
            else if (date.Year > 1600 && date.Year <= 1700)
            {
                powersOfU(y - 1600);
                deltaT = 120 - 0.9808 * u - 0.01532 * u2 + u3 / 7129.0;
            }
            else if(date.Year > 1700 && date.Year <= 1800)
            {
                powersOfU(y - 1700);
                deltaT = 8.83 + 0.1603 * u - 0.0059285 * u2 + 0.00013336 * u3 - u4 / 1174000.0;
            }
            else if(date.Year > 1800 && date.Year <= 1860)
            {
                powersOfU(y - 1800);
                deltaT = 13.72 - 0.332447 * u + 0.0068612 * u2 + 0.0041116 * u3 - 0.00037436 * u4
                    + 0.0000121272 * u5 - 0.0000001699 * u6 + 0.000000000875 * u7;
            }
            else if (date.Year > 1860 && date.Year <= 1900)
            {
                powersOfU(y - 1860);
                deltaT = 7.62 + 0.5737 * u - 0.251754 * u2 + 0.01680668 * u3
                    - 0.0004473624 * u4 + u5 / 233174.0;
            }
            else if (date.Year > 1900 && date.Year <= 1920)
            {
                powersOfU(y - 1900);
                deltaT = -2.79 + 1.494119 * u - 0.0598939 * u2 + 0.0061966 * u3 - 0.000197 * u4;
            }
            else if (date.Year > 1920 && date.Year <= 1941)
            {
                powersOfU(y - 1920);
                deltaT = 21.20 + 0.84493 * u - 0.076100 * u2 + 0.0020936 * u3;
            }
            else if (date.Year > 1941 && date.Year <= 1961)
            {
                powersOfU(y - 1950);
                deltaT = 29.07 + 0.407 * u - u2 / 233.0 + u3 / 2547.0;
            }
            else if (date.Year > 1961 && date.Year <= 1986)
            {
                powersOfU(y - 1975);
                deltaT = 45.45 + 1.067 * u - u2 / 260.0 - u3 / 718.0;
            }
            else if (date.Year > 1986 && date.Year <= 2005)
            {
                powersOfU(y - 2000);
                deltaT = 63.86 + 0.3345 * u - 0.060374 * u2 + 0.0017275 * u3 + 0.000651814 * u4
                    + 0.00002373599 * u5;
            }
            else if (date.Year > 2005 && date.Year <= 2050)
            {
                powersOfU(y - 2000);
                deltaT = 62.92 + 0.32217 * u + 0.005589 * u2;
            }
            else if (date.Year > 2050 && date.Year <= 2150)
            {
                deltaT = -20 + 32 * ((y - 1820) / 100.0) * ((y - 1820) / 100.0) - 0.5628 * (2150 - y);
            }
            else if (date.Year > 2150)
            {
                powersOfU((y - 1820) / 100);
                deltaT = -20 + 32 * u2;
            }
            return deltaT;
        }

        /// <summary>
        /// Calculates mean sidereal time at Greenwich for given instant.
        /// </summary>
        /// <param name="jd">Julian Day</param>
        /// <returns>Mean sidereal time at Greenwich, expressed in degrees.</returns>
        /// <remarks>
        /// AA(II), formula 12.4.
        /// </remarks>
        public static double MeanSiderealTime(double jd)
        {
            double T = (jd - 2451545.0) / 36525.0;
            double T2 = T * T;
            double T3 = T2 * T;

            double theta0 = 280.46061837 + 360.98564736629 * (jd - 2451545.0) +
                0.000387933 * T2 - T3 / 38710000.0;

            theta0 = Angle.To360(theta0);

            return theta0;
        }

        /// <summary>
        /// Calculates apparent sidereal time at Greenwich for given instant.
        /// </summary>
        /// <param name="jd">Julian Day</param>
        /// <returns>Apparent sidereal time at Greenwich, expressed in degrees.</returns>
        /// <remarks>
        /// AA(II), formula 12.4, with corrections for nutation (chapter 22).
        /// </remarks>
        public static double ApparentSiderealTime(double jd, double deltaPsi, double epsilon)
        {
            double cosEpsilon = Math.Cos(Angle.ToRadians(epsilon));
            return MeanSiderealTime(jd) + deltaPsi * cosEpsilon;
        }


        /// <summary>
        /// Calculates the mean obliquity of the ecliptic (ε0) for the given instant.
        /// </summary>
        /// <param name="jd">Julian Day, corresponding to the given date.</param>
        /// <returns>Returns mean obliquity of the ecliptic for the given date, expressed in degrees.</returns>
        /// <remarks>
        /// AA(II) formula 22.3.
        /// </remarks>
        public static double MeanObliquity(double jd)
        {
            double T = (jd - 2451545) / 36525.0;

            double[] U = new double[11];
            double[] c = new double[] { 84381.448, -4680.93, -1.55, +1999.25, -51.38, -249.67, -39.05, +7.12, +27.87, +5.79, +2.45 };

            U[0] = 1;
            U[1] = T / 100.0;
            for (int i = 2; i <= 10; i++)
            {
                U[i] = U[i - 1] * U[1];
            }

            double epsilon0 = 0;
            for (int i = 0; i <= 10; i++)
            {
                epsilon0 += c[i] * U[i];
            }

            return epsilon0 / 3600.0;
        }

        /// <summary>
        /// Calculates the true obliquity of the ecliptic (ε).
        /// </summary>
        /// <param name="jd">Julian Day, corresponding to the given date.</param>
        /// <returns>Returns true obliquity of the ecliptic for the given date, expressed in degrees.</returns>
        /// <remarks>
        /// AA(II) chapter 22.
        /// </remarks>
        public static double TrueObliquity(double jd, double deltaEpsilon)
        {
            return MeanObliquity(jd) + deltaEpsilon;
        }

        #endregion Static methods
    }
}
