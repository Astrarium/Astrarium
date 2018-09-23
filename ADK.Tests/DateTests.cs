using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADK.Tests
{
    [TestClass]
    public class DateTests
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
            // AA2, p.65
            Assert.AreEqual(System.DayOfWeek.Wednesday, new Date(1954, 6, 30).DayOfWeek());
        }
    }
}
