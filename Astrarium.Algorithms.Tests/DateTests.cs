using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Linq;
using System.Globalization;

namespace Astrarium.Algorithms.Tests
{
    [TestClass]
    public class DateTests : TestClassBase
    {
        [TestMethod]
        public void DateToJulianDay()
        {
            // AA2, p.63
            Assert.AreEqual(2451545.0, Date.JulianDay(2000, 1, 1.5));
            Assert.AreEqual(2451179.5, Date.JulianDay(1999, 1, 1));
            Assert.AreEqual(2446822.5, Date.JulianDay(1987, 1, 27));
            Assert.AreEqual(2446966.0, Date.JulianDay(1987, 6, 19.5));
            Assert.AreEqual(2447187.5, Date.JulianDay(1988, 1, 27));
            Assert.AreEqual(2447332.0, Date.JulianDay(1988, 6, 19.5));
            Assert.AreEqual(2415020.5, Date.JulianDay(1900, 1, 1));
            Assert.AreEqual(2305447.5, Date.JulianDay(1600, 1, 1));
            Assert.AreEqual(2305812.5, Date.JulianDay(1600, 12, 31));
            Assert.AreEqual(2026871.8, Date.JulianDay(837, 4, 10.3));
            Assert.AreEqual(1676496.5, Date.JulianDay(-123, 12, 31));
            Assert.AreEqual(1676497.5, Date.JulianDay(-122, 1, 1));
            Assert.AreEqual(1356001.0, Date.JulianDay(-1000, 7, 12.5));
            Assert.AreEqual(1355866.5, Date.JulianDay(-1000, 2, 29));
            Assert.AreEqual(1355671.4, Date.JulianDay(-1001, 8, 17.9));
            Assert.AreEqual(0, Date.JulianDay(-4712, 1, 1.5));
        }

        [TestMethod]
        public void JulianDayToDate()
        {
            // AA2, example 7.c (p.64)
            Assert.AreEqual(new Date(1957, 10, 4.81), new Date(2436116.31));
            Assert.AreEqual(new Date(333, 1, 27.5), new Date(1842713.0));
            Assert.AreEqual(new Date(-584, 5, 28.63), new Date(1507900.13));
        }

        [TestMethod]
        public void DateToString()
        {
            Assert.AreEqual("1957 January 27.5", new Date(1957, 1, 27.5).ToString());
            Assert.AreEqual("-333 October 17.51", new Date(-333, 10, 17.5124).ToString());
        }

        [TestMethod]
        public void IsLeapYear()
        {
            // AA2, p.62
            Assert.IsTrue(Date.IsLeapYear(900));
            Assert.IsTrue(Date.IsLeapYear(1236));
            Assert.IsFalse(Date.IsLeapYear(750));
            Assert.IsFalse(Date.IsLeapYear(1429));
            Assert.IsFalse(Date.IsLeapYear(1700));
            Assert.IsFalse(Date.IsLeapYear(1800));
            Assert.IsFalse(Date.IsLeapYear(1900));
            Assert.IsFalse(Date.IsLeapYear(2100));
            Assert.IsTrue(Date.IsLeapYear(1600));
            Assert.IsTrue(Date.IsLeapYear(2000));
            Assert.IsTrue(Date.IsLeapYear(2400));
        }

        [TestMethod]
        public void IntervalInDays()
        {
            // AA2, p.64
            Assert.AreEqual(27689, new Date(1986, 2, 9) - new Date(1910, 4, 20));

            // AA2, p.64. Answer from the book contains error: year should be 2008.
            Assert.AreEqual(new Date(2008, 11, 26), new Date(1981, 7, 11) + 10000);
        }

        [TestMethod]
        public void DayOfWeek()
        {
            // AA2, p.65, ex. 7.e
            Assert.AreEqual(System.DayOfWeek.Wednesday, new Date(1954, 6, 30).DayOfWeek());
        }

        [TestMethod]
        public void DayOfYear()
        {
            // AA2, p.65, ex. 7.f
            Assert.AreEqual(318, Date.DayOfYear(new Date(1978, 11, 14)));

            // AA2, p.65, ex. 7.g
            Assert.AreEqual(113, Date.DayOfYear(new Date(1988, 4, 22)));
        }

        [TestMethod]
        public void DaysInMonth()
        {
            Assert.AreEqual(31, Date.DaysInMonth(2017, 1));
            Assert.AreEqual(28, Date.DaysInMonth(2017, 2));
            Assert.AreEqual(31, Date.DaysInMonth(2017, 3));
            Assert.AreEqual(30, Date.DaysInMonth(2017, 4));
            Assert.AreEqual(31, Date.DaysInMonth(2017, 5));
            Assert.AreEqual(30, Date.DaysInMonth(2017, 6));
            Assert.AreEqual(31, Date.DaysInMonth(2017, 7));
            Assert.AreEqual(31, Date.DaysInMonth(2017, 8));
            Assert.AreEqual(30, Date.DaysInMonth(2017, 9));
            Assert.AreEqual(31, Date.DaysInMonth(2017, 10));
            Assert.AreEqual(30, Date.DaysInMonth(2017, 11));
            Assert.AreEqual(31, Date.DaysInMonth(2017, 12));
            Assert.AreEqual(28, Date.DaysInMonth(1900, 2));
            Assert.AreEqual(29, Date.DaysInMonth(2000, 2));
            Assert.AreEqual(29, Date.DaysInMonth(2020, 2));
            Assert.AreEqual(28, Date.DaysInMonth(2100, 2));
        }

        [TestMethod]
        public void GregorianEaster()
        {
            // AA2, p.68
            Assert.AreEqual(new Date(1991, 3, 31), Date.GregorianEaster(1991));
            Assert.AreEqual(new Date(1992, 4, 19), Date.GregorianEaster(1992));
            Assert.AreEqual(new Date(1993, 4, 11), Date.GregorianEaster(1993));
            Assert.AreEqual(new Date(1954, 4, 18), Date.GregorianEaster(1954));
            Assert.AreEqual(new Date(2000, 4, 23), Date.GregorianEaster(2000));
            Assert.AreEqual(new Date(1818, 3, 22), Date.GregorianEaster(1818));
        }

        [TestMethod]
        public void JulianEaster()
        {
            // Dates of Easter Sunday in Julian calendar are taken from
            // http://www.kevinlaughery.com/east4099.html

            Assert.AreEqual(new Date(375, 4, 5), Date.JulianEaster(375));
            Assert.AreEqual(new Date(561, 4, 17), Date.JulianEaster(561));
            Assert.AreEqual(new Date(1483, 3, 30), Date.JulianEaster(1483));
            Assert.AreEqual(new Date(1570, 3, 26), Date.JulianEaster(1570));

            // Some tests for modern (Gregorian Calendar) dates of Orthodox Chirstian Easter.
            // Dates are taken from: https://en.wikipedia.org/wiki/List_of_dates_for_Easter

            Assert.AreEqual(new Date(2004, 4, 11), Date.JulianEaster(2004).ToGregorianCalendarDate());
            Assert.AreEqual(new Date(2005, 5, 1), Date.JulianEaster(2005).ToGregorianCalendarDate());
            Assert.AreEqual(new Date(2030, 4, 28), Date.JulianEaster(2030).ToGregorianCalendarDate());
            Assert.AreEqual(new Date(2051, 5, 7), Date.JulianEaster(2051).ToGregorianCalendarDate());
        }

        [TestMethod]
        public void GregorianToJulianCalendarDate()
        {
            Assert.AreEqual(new Date(1582, 10, 5), Date.GregorianToJulian(new Date(1582, 10, 15)));
            Assert.AreEqual(new Date(400, 2, 4), Date.GregorianToJulian(new Date(400, 2, 5)));
            Assert.AreEqual(new Date(2018, 9, 12), Date.GregorianToJulian(new Date(2018, 9, 25)));
            Assert.AreEqual(new Date(2018, 12, 29), Date.GregorianToJulian(new Date(2019, 1, 11)));
            Assert.AreEqual(new Date(2016, 12, 31), Date.GregorianToJulian(new Date(2017, 1, 13)));
            Assert.AreEqual(new Date(2015, 12, 31), Date.GregorianToJulian(new Date(2016, 1, 13)));
            Assert.AreEqual(new Date(2015, 12, 30), Date.GregorianToJulian(new Date(2016, 1, 12)));
            Assert.AreEqual(new Date(1900, 1, 1), Date.GregorianToJulian(new Date(1900, 1, 13)));
            Assert.AreEqual(new Date(1899, 12, 31), Date.GregorianToJulian(new Date(1900, 1, 12)));
            Assert.AreEqual(new Date(1904, 2, 16), Date.GregorianToJulian(new Date(1904, 2, 29)));
            Assert.AreEqual(new Date(1000, 2, 23), Date.GregorianToJulian(new Date(1000, 2, 28)));
            Assert.AreEqual(new Date(1000, 2, 23), Date.GregorianToJulian(new Date(1000, 2, 28)));
            Assert.AreEqual(new Date(1000, 2, 24), Date.GregorianToJulian(new Date(1000, 3, 1)));
            Assert.AreEqual(new Date(1000, 2, 28), Date.GregorianToJulian(new Date(1000, 3, 5)));
            Assert.AreEqual(new Date(1000, 2, 29), Date.GregorianToJulian(new Date(1000, 3, 6)));
            Assert.AreEqual(new Date(1000, 12, 25), Date.GregorianToJulian(new Date(1000, 12, 31)));
        }

        [TestMethod]
        public void JulianToGregorianCalendarDate()
        {
            Assert.AreEqual(new Date(1582, 10, 15), Date.JulianToGregorian(new Date(1582, 10, 5)));
            Assert.AreEqual(new Date(400, 2, 5), Date.JulianToGregorian(new Date(400, 2, 4)));
            Assert.AreEqual(new Date(2018, 9, 25), Date.JulianToGregorian(new Date(2018, 9, 12)));
            Assert.AreEqual(new Date(2019, 1, 11), Date.JulianToGregorian(new Date(2018, 12, 29)));
            Assert.AreEqual(new Date(2017, 1, 13), Date.JulianToGregorian(new Date(2016, 12, 31)));
            Assert.AreEqual(new Date(2016, 1, 13), Date.JulianToGregorian(new Date(2015, 12, 31)));
            Assert.AreEqual(new Date(2016, 1, 12), Date.JulianToGregorian(new Date(2015, 12, 30)));
            Assert.AreEqual(new Date(1900, 1, 13), Date.JulianToGregorian(new Date(1900, 1, 1)));
            Assert.AreEqual(new Date(1900, 1, 12), Date.JulianToGregorian(new Date(1899, 12, 31)));
            Assert.AreEqual(new Date(1904, 2, 29), Date.JulianToGregorian(new Date(1904, 2, 16)));
            Assert.AreEqual(new Date(1000, 2, 28), Date.JulianToGregorian(new Date(1000, 2, 23)));
            Assert.AreEqual(new Date(1000, 2, 28), Date.JulianToGregorian(new Date(1000, 2, 23)));
            Assert.AreEqual(new Date(1000, 3, 1), Date.JulianToGregorian(new Date(1000, 2, 24)));
            Assert.AreEqual(new Date(1000, 3, 5), Date.JulianToGregorian(new Date(1000, 2, 28)));
            Assert.AreEqual(new Date(1000, 3, 6), Date.JulianToGregorian(new Date(1000, 2, 29)));
            Assert.AreEqual(new Date(1000, 12, 31), Date.JulianToGregorian(new Date(1000, 12, 25)));
        }

        [TestMethod]
        public void DeltaT()
        {
            NumberFormatInfo numericFormat = new NumberFormatInfo();
            numericFormat.NumberDecimalSeparator = ".";

            var testValues = ReadLinesFromResource("Astrarium.Algorithms.Tests.Data.DeltaT.chk", Encoding.UTF8)
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.Trim().StartsWith("#"))
                .Select(line =>
                {
                    string[] chunks = line.Split(';');
                    return new
                    {
                        Year = Int32.Parse(chunks[0].Trim(), numericFormat),
                        DeltaT = Double.Parse(chunks[1].Trim(), numericFormat)
                    };
                })
                .ToArray();

            foreach (var testValue in testValues)
            {
                for (int m = 1; m <= 12; m++)
                {
                    double expected = testValue.DeltaT;
                    double actual = new Date(testValue.Year, m, 1).DeltaT();

                    // difference between expected and actual values, in seconds
                    double diff = Math.Abs(expected - actual);

                    // error of calculation, in percents
                    double error = diff / expected * 100;

                    // suppose absolute difference is less than 1 second,
                    // or error less than 2 percent
                    Assert.IsTrue(diff < 1 || error < 2);
                }
            }
        }

        [TestMethod]
        public void Epochs()
        {
            double expectedError = 1e-4;

            // Test from AA
            Assert.AreEqual(2446431.5, Date.JulianEpoch(1986), expectedError);

            Assert.AreEqual(Date.EPOCH_J1900, Date.JulianEpoch(1900), expectedError);
            Assert.AreEqual(Date.EPOCH_J1950, Date.JulianEpoch(1950), expectedError);
            Assert.AreEqual(Date.EPOCH_J1975, Date.JulianEpoch(1975), expectedError);
            Assert.AreEqual(Date.EPOCH_J2000, Date.JulianEpoch(2000), expectedError);
            Assert.AreEqual(Date.EPOCH_J2050, Date.JulianEpoch(2050), expectedError);
            Assert.AreEqual(Date.EPOCH_B1875, Date.BesselianEpoch(1875), expectedError);
            Assert.AreEqual(Date.EPOCH_B1900, Date.BesselianEpoch(1900), expectedError);
            Assert.AreEqual(Date.EPOCH_B1950, Date.BesselianEpoch(1950), expectedError);
        }

        /// <summary>
        /// AA(II), example 12.a
        /// </summary>
        [TestMethod]
        public void SiderealTime()
        {
            double jd = 2446895.5;

            // Nutation in longitude
            double deltaPsi = -3.788 / 3600;

            // True obliquity
            double epsilon = new DMS("23* 26' 36.85''").ToDecimalAngle();

            // AA(II), example 12.a
            Assert.AreEqual(new HMS("13h 10m 46.3668s"), new HMS(Date.MeanSiderealTime(jd)));
            Assert.AreEqual(new HMS("13h 10m 46.1351s"), new HMS(Date.ApparentSiderealTime(jd, deltaPsi, epsilon)));

            // AA(II), example 12.b
            Assert.AreEqual(128.7378734, Date.MeanSiderealTime(2446896.30625), 1e-6);
        }

        /// <summary>
        /// AA(II), example 22.a.
        /// </summary>
        [TestMethod]
        public void MeanObliquity()
        {
            double jd = 2446895.5;

            // Mean obliquity
            var epsilon0 = Date.MeanObliquity(jd);

            Assert.AreEqual(new DMS("23* 26' 27.407''"), new DMS(epsilon0));
        }

        /// <summary>
        /// AA(II), example 22.a.
        /// </summary>
        [TestMethod]
        public void TrueObliquity()
        {
            double jd = 2446895.5;

            // Nutation in obliquity
            double deltaEpsilon = 9.443 / 3600;

            // True obliquity
            var epsilon = Date.TrueObliquity(jd, deltaEpsilon);

            Assert.AreEqual(new DMS("23* 26' 36.850''").ToDecimalAngle(), epsilon, 1 / 3600.0 / 2);
        }
    }
}
