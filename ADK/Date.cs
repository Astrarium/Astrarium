using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ADK
{
    public class Date
    {
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
        /// Adds specified amount of days to <see cref="Date"/>.
        /// </summary>
        /// <returns></returns>
        public static Date operator +(Date left, double right)
        {
            return new Date(left.ToJulianDay() + right);
        }

        public static Date operator -(Date left, double right)
        {
            return new Date(left.ToJulianDay() - right);
        }

        public double ToJulianDay()
        {
            return JulianDay(this);
        }

        public bool IsLeapYear()
        {
            return IsLeapYear(Year);
        }

        public DayOfWeek DayOfWeek()
        {
            return DayOfWeek(ToJulianDay());
        }

        public static double JulianDay(Date date)
        {
            return JulianDay(date.Year, date.Month, date.Day);
        }

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

        public static DayOfWeek DayOfWeek(double jd)
        {
            double d = jd + 1.5;
            return (DayOfWeek)((int)d % 7);
        }

        public static int DayOfYear(Date date)
        {
            int K = IsLeapYear(date.Year) ? 1 : 2;
            return (int)((275 * date.Month / 9) - K * ((date.Month + 9) / 12) + date.Day - 30);
        }

        private static int[] DAYS_IN_MONTH = new int[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        public static int DaysInMonth(int year, int month)
        {
            if (month < 0 || month > 12)
                throw new ArgumentException("Month value should be from 1 to 12.", nameof(month));

            return DAYS_IN_MONTH[month] + month == 2 ? (IsLeapYear(year) ? 1 : 0) : 0;
        }
    }
}
