using System;
using System.Globalization;

namespace ADK
{
    public class Date
    {
        /// <summary>
        /// Epoch B1875.0 in julian days.
        /// </summary>
        public const double EPOCH_B1875 = 2405889.5;

        /// <summary>
        /// Epoch J1975.0 in julian days.
        /// </summary>
        public const double EPOCH_J1975 = 2442412.5;

        /// <summary>
        /// Epoch B1950.0 in julian days.
        /// </summary>
        public const double EPOCH_B1950 = 2433282.4235;

        /// <summary>
        /// Epoch J2000.0 in julian days.
        /// </summary>
        public const double EPOCH_J2000 = 2451545.0;

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

        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

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

        public Date(DateTime dateTime)
        {
            DateTime uDateTime = dateTime.ToUniversalTime();
            Year = uDateTime.Year;
            Month = uDateTime.Month;
            Day = uDateTime.Day + uDateTime.TimeOfDay.TotalHours / 24.0;
        }

        public static Date Now
        {
            get
            {
                return new Date(DateTime.Now);
            }
        }

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

        public override string ToString()
        {
            string month = Culture.DateTimeFormat.GetMonthName(Month);
            string day = Day.ToString("0.##",Culture.NumberFormat);
            return string.Format($"{Year} {month} {day}");
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

        /// <summary>
        /// Converts the <see cref="Date"/> to Julian Day value.
        /// </summary>
        /// <returns>Julian Day value.</returns>
        public double ToJulianDay()
        {
            return JulianDay(this);
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

        #region Static methods

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
        /// <param name="D">Day of month.</param>
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
            if (month < 0 || month > 12)
                throw new ArgumentException("Month value should be from 1 to 12.", nameof(month));

            return DAYS_IN_MONTH[month] + month == 2 ? (IsLeapYear(year) ? 1 : 0) : 0;
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

        #endregion Static methods
    }
}
