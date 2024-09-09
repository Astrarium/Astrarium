using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Astrarium.Algorithms.Tests
{
    [TestClass]
    public class SolarEphemTests
    {
        /// <summary>
        /// Synthetic tests, solar semidiameter at 1 au, aphelion and perihelion.
        /// </summary>
        [TestMethod]
        public void Semidiameter()
        {
            Assert.AreEqual(959.63, SolarEphem.Semidiameter(1), 1e-2);
            Assert.AreEqual(943.87, SolarEphem.Semidiameter(1.0167), 1e-2);
            Assert.AreEqual(975.94, SolarEphem.Semidiameter(0.98329), 1e-2);
        }

        /// <summary>
        /// Synthetic tests, solar parallax in arc seconds at 1 au, aphelion and perihelion.
        /// </summary>
        [TestMethod]
        public void Parallax()
        {
            Assert.AreEqual(8.794, SolarEphem.Parallax(1) * 3600, 1e-3);
            Assert.AreEqual(8.650, SolarEphem.Parallax(1.0167) * 3600, 1e-3);
            Assert.AreEqual(8.943, SolarEphem.Parallax(0.98329) * 3600, 1e-3);
        }

        /// <summary>
        /// Test from Duffeth-Smith book, p. 75
        /// </summary>
        [TestMethod]
        public void CarringtonNumber()
        {
            Assert.AreEqual(1624, SolarEphem.CarringtonNumber(2442439.50));
        }

        /// <summary>
        /// Test from AA(I), example 26.a and table 26.e
        /// </summary>
        [TestMethod]
        public void Seasons()
        {
            // 2 min error
            const double error = 2.0 / (24 * 60);

            // example 26.a
            {
                double jd = SolarEphem.Season(new Date(1962, 1, 1).ToJulianDay(), Season.Summer);
                Assert.AreEqual(2437837.39245, jd, error);
            }

            {
                double jd = SolarEphem.Season(new Date(1991, 1, 1).ToJulianDay(), Season.Spring);
                Assert.AreEqual(new Date(new DateTime(1991, 3, 21, 3, 2, 54, DateTimeKind.Utc)).ToJulianEphemerisDay(), jd, error);
            }

            {
                double jd = SolarEphem.Season(new Date(1991, 1, 1).ToJulianDay(), Season.Summer);
                Assert.AreEqual(new Date(new DateTime(1991, 6, 21, 21, 19, 46, DateTimeKind.Utc)).ToJulianEphemerisDay(), jd, error);
            }

            {
                double jd = SolarEphem.Season(new Date(1991, 1, 1).ToJulianDay(), Season.Autumn);
                Assert.AreEqual(new Date(new DateTime(1991, 9, 23, 12, 49, 04, DateTimeKind.Utc)).ToJulianEphemerisDay(), jd, error);
            }

            {
                double jd = SolarEphem.Season(new Date(1991, 1, 1).ToJulianDay(), Season.Winter);
                Assert.AreEqual(new Date(new DateTime(1991, 12, 22, 8, 54, 38, DateTimeKind.Utc)).ToJulianEphemerisDay(), jd, error);
            }
        }

        /// <summary>
        /// AA(II), example 25.a
        /// </summary>
        [TestMethod]
        public void Ecliptical()
        {
            var ecl = SolarEphem.Ecliptical(2448908.5);

            Assert.AreEqual(new DMS("199* 54' 32.19''"), new DMS(ecl.Lambda));
            Assert.AreEqual(0.99766, ecl.Distance, 1e-5);
        }

        /// <summary>
        /// AA(II), example 29.a
        /// </summary>
        [TestMethod]
        public void Center_1()
        {
            var hel = SolarEphem.Center(2448908.50068);

            Assert.AreEqual(5.99, hel.Latitude, 1e-2);
            Assert.AreEqual(238.63, hel.Longitude, 1e-2);
        }

        /// <summary>
        /// Test from Duffeth-Smith book, ex. 37A (p. 73)
        /// </summary>
        [TestMethod]
        public void Center_2()
        {
            var hel = SolarEphem.Center(2443994.5);

            Assert.AreEqual(-4.196, hel.Latitude, 1e-2);
            Assert.AreEqual(301.097, hel.Longitude, 1e-2);
        }
    }
}
