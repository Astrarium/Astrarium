using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Astrarium.Algorithms.Tests
{
    [TestClass]
    public class LunarEphemTests
    {
        /// <summary>
        /// AA(II), example 48.a
        /// </summary>
        [TestMethod]
        public void Phase_PhaseAngle_Elongation()
        {
            CrdsEquatorial eqSun = new CrdsEquatorial(20.6579, 8.6964);
            CrdsEquatorial eqMoon = new CrdsEquatorial(134.6855, 13.7684);

            const double epslion = 23.440636;

            CrdsEcliptical sun = eqSun.ToEcliptical(epslion);
            sun.Distance = 149971520;

            CrdsEcliptical moon = eqMoon.ToEcliptical(epslion);
            moon.Distance = 368410;

            double psi = BasicEphem.Elongation(sun, moon);
            Assert.AreEqual(110.79, psi, 1e-2);

            double phaseAngle = BasicEphem.PhaseAngle(psi, sun.Distance, moon.Distance);
            Assert.AreEqual(69.08, phaseAngle, 1e-2);

            double phase = BasicEphem.Phase(phaseAngle);
            Assert.AreEqual(0.68, phase, 1e-2);
        }

        /// <summary>
        /// AA(II), example 47.a
        /// </summary>
        [TestMethod]
        public void Parallax()
        {
            double pi = LunarEphem.Parallax(368409.7);
            Assert.AreEqual(0.991990, pi, 1e-6);
        }

        /// <summary>
        /// PAWC, ex. 55 (p. 111)
        /// </summary>
        [TestMethod]
        public void PositionAngleOfBrightLimb()
        {
            CrdsEquatorial eqMoon = new CrdsEquatorial(new HMS("14h 35m 02s"), new DMS("-12* 45' 46''"));
            CrdsEquatorial eqSun = new CrdsEquatorial(15.843611 * 15, -20.117778);

            double pa = LunarEphem.PositionAngleOfBrightLimb(eqSun, eqMoon);
            Assert.AreEqual(114.6, pa, 0.1);
        }

        /// <summary>
        /// AA(II), p. 374, example 53.a.
        /// </summary>
        [TestMethod]
        public void PositionAngleOfAxis()
        {
            double jd = 2448724.5;
            CrdsEcliptical ecl = new CrdsEcliptical(new DMS("133* 10' 00''"), new DMS("-3* 13' 45''"));
            double epsilon = 23.440636;
            double deltaPsi = 0.004610;

            double P = LunarEphem.PositionAngleOfAxis(jd, ecl, epsilon, deltaPsi);
            Assert.AreEqual(15.08, P, 1e-2);
        }

        /// <summary>
        /// AA(II), p. 374, example 53.a.
        /// </summary>
        [TestMethod]
        public void Libration()
        {
            double jd = 2448724.5;
            CrdsEcliptical ecl = new CrdsEcliptical(133.167260, -3.229127);
            double deltaPsi = 0.004610;

            Libration le = LunarEphem.Libration(jd, ecl, deltaPsi);
            Assert.AreEqual(-1.23, le.l, 1e-2);
            Assert.AreEqual(4.20, le.b, 1e-2);
        }

        /// <summary>
        /// Expected libration data are taken from https://www.calsky.com/?moonevents=&tdt=2458533
        /// </summary>
        [TestMethod]
        public void NearestLibration()
        {
            // 10 minutes
            double timeAccuracy = TimeSpan.FromMinutes(10).TotalDays;

            // 0.1 degree
            double angleAccuracy = 1e-1;

            // starting date
            double jd0 = new Date(new DateTime(2019, 3, 1, 0, 0, 0, DateTimeKind.Utc)).ToJulianDay();

            // libration angle value
            double value = 0;

            // Saturday 23 February 2019, 08:40 UTC, Max. Libration South: -6.715°
            {
                double jdMaxExpected = new Date(new DateTime(2019, 2, 23, 8, 40, 0, DateTimeKind.Utc)).ToJulianEphemerisDay();
                double valueExpected = -6.715;

                double jd = LunarEphem.NearestMaxLibration(jd0, LibrationEdge.South, out value);

                Assert.AreEqual(jdMaxExpected, jd, timeAccuracy);
                Assert.AreEqual(valueExpected, value, angleAccuracy);
            }

            // Monday 25 February 2019, 11:05 UTC, Max. Libration East: 7.770°
            {
                double jdMaxExpected = new Date(new DateTime(2019, 2, 25, 11, 05, 0, DateTimeKind.Utc)).ToJulianEphemerisDay();
                double valueExpected = 7.770;

                double jd = LunarEphem.NearestMaxLibration(jd0, LibrationEdge.East, out value);

                Assert.AreEqual(jdMaxExpected, jd, timeAccuracy);
                Assert.AreEqual(valueExpected, value, angleAccuracy);
            }

            // Sunday 09 March 2019, 21:15 UTC, Max. Libration North: +6.656°
            {
                double jdMaxExpected = new Date(new DateTime(2019, 3, 9, 21, 15, 0, DateTimeKind.Utc)).ToJulianEphemerisDay();
                double valueExpected = 6.656;

                double jd = LunarEphem.NearestMaxLibration(jd0, LibrationEdge.North, out value);

                Assert.AreEqual(jdMaxExpected, jd, timeAccuracy);
                Assert.AreEqual(valueExpected, value, angleAccuracy);
            }

            // Wednesday 13 March 2019, 07:23 UTC, Max. Libration West: -6.961°
            {
                double jdMaxExpected = new Date(new DateTime(2019, 3, 13, 7, 23, 0, DateTimeKind.Utc)).ToJulianEphemerisDay();
                double valueExpected = -6.961;

                double jd = LunarEphem.NearestMaxLibration(jd0, LibrationEdge.West, out value);

                Assert.AreEqual(jdMaxExpected, jd, timeAccuracy);
                Assert.AreEqual(valueExpected, value, angleAccuracy);
            }
        }

        /// <summary>
        /// AA(II), p. 353, examples 49.a, 49.b.
        /// </summary>
        [TestMethod]
        public void NearestPhase()
        {
            // 1 second error
            double error = 1.0 / (24 * 60 * 60);

            {
                // New Moon takes place in February 1977
                Date date = new Date(1977, 2, 15);
                double jd = date.ToJulianEphemerisDay();

                double jdNewMoon = LunarEphem.NearestPhase(jd, MoonPhase.NewMoon);

                Assert.AreEqual(2443192.65118, jdNewMoon, error);
            }

            {
                // First last quarter of 2044
                Date date = new Date(2044, 1, 1);
                double jd = date.ToJulianEphemerisDay();

                double jdLastQuarter = LunarEphem.NearestPhase(jd, MoonPhase.LastQuarter);

                Assert.AreEqual(2467636.49186, jdLastQuarter, error);
            }
        }

        /// <summary>
        /// AA(II), p. 357, example 50.a.
        /// </summary>
        [TestMethod]
        public void NearestApsis()
        {
            Date date = new Date(1988, 10, 1);
            double jd = date.ToJulianEphemerisDay();

            double diameter = 0;
            // TODO: check diameter
            double jdApogee = LunarEphem.NearestApsis(jd, MoonApsis.Apogee, out diameter);

            // 1 minute error
            double error = 1.0 / (24 * 60);

            Assert.AreEqual(2447442.3537, jdApogee, error);
        }

        /// <summary>
        /// AA(II), p. 357, chapter 52.
        /// </summary>
        [TestMethod]
        public void NearestDeclination()
        {
            // 1 minute error
            double error = TimeSpan.FromMinutes(1).TotalDays;

            // output declination value
            double delta;

            // example 52.a
            {
                Date date = new Date(1988, 12, 15);
                double jd = date.ToJulianEphemerisDay();
                double jdMaxDeclination = LunarEphem.NearestMaxDeclination(jd, MoonDeclination.North, out delta);

                Assert.AreEqual(2447518.3347, jdMaxDeclination, error);
                Assert.AreEqual(28.1562, delta, 1e-4);
            }

            // example 52.b
            {
                Date date = new Date(2049, 4, 15);
                double jd = date.ToJulianEphemerisDay();
                double jdMaxDeclination = LunarEphem.NearestMaxDeclination(jd, MoonDeclination.South, out delta);
                Assert.AreEqual(2469553.0834, jdMaxDeclination, error);
                Assert.AreEqual(22.1384, delta, 1e-4);
            }

            // example 52.c
            {
                Date date = new Date(-4, 3, 15);
                double jd = date.ToJulianEphemerisDay();
                double jdMaxDeclination = LunarEphem.NearestMaxDeclination(jd, MoonDeclination.North, out delta);
                // for the dates between -1000 and +500 maximal error in time will not exceed half an hour actually
                Assert.AreEqual(1719672.1337, jdMaxDeclination, TimeSpan.FromMinutes(15).TotalDays);
                Assert.AreEqual(28.9739, delta, 1e-4);
            }
        }

        [TestMethod]
        public void LunationNumber()
        {            
            Assert.AreEqual(1217, LunarEphem.Lunation(new Date(2021, 5, 11).ToJulianEphemerisDay()));
            Assert.AreEqual(-282, LunarEphem.Lunation(new Date(1900, 2, 17).ToJulianEphemerisDay()));
        }

        [TestMethod]
        public void Noumenia()
        {
            // Example are taken from
            // http://www.makkahcalendar.org/en/islamicCalendarArticle4.php

            // IX.4 Example of Makkah
            {
                // 17 Nov 2009, "best time" is 14:48 UTC
                double jd = new Date(new DateTime(2009, 11, 17, 14, 48, 0, DateTimeKind.Utc)).ToJulianEphemerisDay();

                // Nutation elements
                var nutation = Nutation.NutationElements(jd);

                // True obliquity
                var epsilon = Date.TrueObliquity(jd, nutation.deltaEpsilon);

                // Sidereal time at Greenwich
                double siderealTime = Date.ApparentSiderealTime(jd, nutation.deltaPsi, epsilon);

                // Ecliptical coordinates of the Sun, taken from the example (see ref.)
                CrdsEcliptical eclSun = new CrdsEcliptical(235.39, 0);

                // Ecliptical coordinates of the Moon, taken from the example (see ref.)
                CrdsEcliptical eclMoon = new CrdsEcliptical(245.01, -3.76);

                // Geograhical coordinates of Makkah
                CrdsGeographical geo = new CrdsGeographical(-39.82563, 21.42664);

                double q = LunarEphem.CrescentQ(eclMoon, eclSun, 0.2539 * 3600, epsilon, siderealTime, geo);

                Assert.AreEqual(-0.465, q, 0.001);
            }

            // IX.5 Example of intermediate horizon 30°W, 30°S at 21:04
            {
                // 17 Nov 2009, 21:04 UTC
                double jd = new Date(new DateTime(2009, 11, 17, 21, 04, 0, DateTimeKind.Utc)).ToJulianEphemerisDay();

                // Nutation elements
                var nutation = Nutation.NutationElements(jd);

                // True obliquity
                var epsilon = Date.TrueObliquity(jd, nutation.deltaEpsilon);

                // Sidereal time at Greenwich
                double siderealTime = Date.ApparentSiderealTime(jd, nutation.deltaPsi, epsilon);

                // Ecliptical coordinates of the Sun, taken from the example (see ref.)
                CrdsEcliptical eclSun = new CrdsEcliptical(235.65, 0);

                // Ecliptical coordinates of the Moon, taken from the example (see ref.)
                CrdsEcliptical eclMoon = new CrdsEcliptical(248.32, -3.55);

                // Geograhical coordinates of Makkah
                CrdsGeographical geo = new CrdsGeographical(30, -30);

                double q = LunarEphem.CrescentQ(eclMoon, eclSun, 0.2536 * 3600, epsilon, siderealTime, geo);

                Assert.AreEqual(0.367, q, 0.001);
            }
        }
    }
}
