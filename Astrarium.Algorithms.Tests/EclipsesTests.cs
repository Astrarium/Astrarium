using Astrarium.Algorithms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Astrarium.Algorithms.Tests
{
    [TestClass]
    public class EclipsesTests
    {
        [TestMethod]
        public void FindInstantBesselianElements()
        {
            // 11 Aug 1999, 11:00:00 TDT
            // Besselian elements are taken from
            // https://eclipse.gsfc.nasa.gov/SEsearch/SEdata.php?Ecl=+19990811
            // Sun and Moon geocentric positions are taken
            // from NASA horizons tool
            // https://ssd.jpl.nasa.gov/horizons.cgi

            var position = new SunMoonPosition()
            {
                JulianDay = 2451401.958333333,
                Sun = new CrdsEquatorial(new HMS("09h 23m 11.27s"), new DMS("+15* 19' 30.1''")),
                Moon = new CrdsEquatorial(new HMS("09h 23m 30.71s"), new DMS("+15* 48' 50.3''")),
                DistanceSun = 1.5162934697E+08 / 6371.0,
                DistanceMoon = 3.7330565613E+05 / 6371.0
            };

            var elements = SolarEclipses.BesselianElements(position);

            Assert.AreEqual(15.32734, elements.D, 1e-2);
            Assert.AreEqual(343.68741, elements.Mu, 1e-1);
            Assert.AreEqual(0.070042, elements.X, 1e-2);
            Assert.AreEqual(0.502841, elements.Y, 1e-2);
            Assert.AreEqual(0.542469, elements.L1, 1e-2);
            Assert.AreEqual(-0.003650, elements.L2, 1e-2);
        }

        /// <summary>
        /// AA(II), ex. 54a
        /// </summary>
        [TestMethod]
        public void FindNearestSolarEclipse()
        {
            double jd = new Date(1993, 5, 21).ToJulianDay();
            SolarEclipse eclipse = SolarEclipses.NearestEclipse(jd, true);

            Assert.AreEqual(2449129.0979, eclipse.JulianDayMaximum, TimeSpan.FromMinutes(0.36).TotalDays);
            Assert.AreEqual(SolarEclipseType.Partial, eclipse.EclipseType);
            Assert.AreEqual(0.740, eclipse.Magnitude, 1e-3);
            Assert.AreEqual(SolarEclipseRegio.Northern, eclipse.Regio);
        }

        /// <summary>
        /// AA(II), ex. 54c / 54d
        /// </summary>
        [TestMethod]
        public void FindNearestLunarEclipse()
        {
            // ex. 54c
            {
                double jd = new Date(1973, 6, 1).ToJulianDay();
                LunarEclipse eclipse = LunarEclipses.NearestEclipse(jd, true);
                var d = new Date(eclipse.JulianDayMaximum, 0);
                Assert.AreEqual(1973, d.Year);
                Assert.AreEqual(6, d.Month);
                Assert.AreEqual(15, (int)d.Day);
                Assert.AreEqual(20, d.Hour);
                Assert.AreEqual(50, d.Minute);
                Assert.AreEqual(LunarEclipseType.Penumbral, eclipse.EclipseType);
                Assert.AreEqual(0.462, eclipse.Magnitude, 1e-3);
            }

            // ex. 54d
            {
                double jd = new Date(1997, 7, 1).ToJulianDay();
                LunarEclipse eclipse = LunarEclipses.NearestEclipse(jd, true);
                var d = new Date(eclipse.JulianDayMaximum, 0);
                Assert.AreEqual(1997, d.Year);
                Assert.AreEqual(9, d.Month);
                Assert.AreEqual(16, (int)d.Day);
                Assert.AreEqual(18, d.Hour);
                Assert.AreEqual(47, d.Minute);
                Assert.AreEqual(LunarEclipseType.Total, eclipse.EclipseType);
                Assert.AreEqual(1.187, eclipse.Magnitude, 1e-3);
            }
        }

        /// <summary>
        /// Example 5 (EOSE)
        /// </summary>
        [TestMethod]
        public void PointOfCentralEclipse()
        {
            // 10 may 1994
            var pbe = new PolynomialBesselianElements()
            {
                JulianDay0 = 2449483.20833,
                X = new[] { -0.173_367, +0.499_0629, +0.000_0296, -0.000_005_63 },
                Y = new[] { +0.383_484, +0.086_9393, -0.000_1183, -0.000_000_92 },
                D = new[] { +17.686_13, +0.010_642, -0.000_004, 0 },
                Mu = new[] { 75.909_23, 15.001_621, 0, 0 },
                L1 = new[] { +0.566_906, -0.000_0318, -0.000_0098, 0 },
                L2 = new[] { +0.020_679, -0.000_0317, -0.000_0097, 0 },
                TanF1 = 0.004_6308,
                TanF2 = 0.004_6077,
                Step = 1.0 / 24.0,                                
                DeltaT = 61 // Used in examples from Meeus' book
            };

            // 1 second of time is enough
            double epsTime = TimeSpan.FromSeconds(1).TotalDays;

            // 1 second of arc is enough
            double epsCoord = 1 / 3600.0;

            // Example 5
            {
                var pointInitial = new CrdsGeographical(97, 0);
                var pointFinal = SolarEclipses.FindEclipseCurvePoint(pbe, pointInitial);

                // 16:35:50 UT, convert to DT!
                double expectedTime = (16 + 35 / 60.0 + 50 / 3600.0 + pbe.DeltaT / 3600.0) / 24.0;

                Assert.AreEqual(36.74052, pointFinal.Latitude, epsCoord);
                Assert.AreEqual(expectedTime, new Date(pointFinal.JulianDay).Time, epsTime);
            }

            // Example 6 (Northern limit of annlar path, point for longitude 97 West)
            {
                var pointInitial = new CrdsGeographical(97, 0);
                var pointFinal = SolarEclipses.FindEclipseCurvePoint(pbe, pointInitial, 1, 1);

                // 16:38:05 UT, convert to DT!
                double expectedTime = (16 + 38 / 60.0 + 05 / 3600.0 + pbe.DeltaT / 3600.0) / 24.0;

                Assert.AreEqual(37.99034, pointFinal.Latitude, epsCoord);
                Assert.AreEqual(expectedTime, new Date(pointFinal.JulianDay).Time, epsTime);
            }

            // Example 6 (Southern limit of annular path, point for longitude 97 West)
            {
                var pointInitial = new CrdsGeographical(97, 0);
                var pointFinal = SolarEclipses.FindEclipseCurvePoint(pbe, pointInitial, -1, 1);

                // 16:33:34 UT, convert to DT!
                double expectedTime = (16 + 33 / 60.0 + 34 / 3600.0 + pbe.DeltaT / 3600.0) / 24.0;

                Assert.AreEqual(35.49901, pointFinal.Latitude, epsCoord);
                Assert.AreEqual(expectedTime, new Date(pointFinal.JulianDay).Time, epsTime);
            }

            // Excercise from page 24 (1)
            {
                var pointInitial = new CrdsGeographical(120, 0);
                var pointFinal = SolarEclipses.FindEclipseCurvePoint(pbe, pointInitial, 1, 0.8);

                // 15:56:17 UT, convert to DT!
                double expectedTime = (15 + 56 / 60.0 + 17 / 3600.0 + pbe.DeltaT / 3600.0) / 24.0;

                Assert.AreEqual(32.7424, pointFinal.Latitude, epsCoord);
                Assert.AreEqual(expectedTime, new Date(pointFinal.JulianDay).Time, epsTime);
            }

            // Excercise from page 24 (2)
            {
                var pointInitial = new CrdsGeographical(115, 0);
                var pointFinal = SolarEclipses.FindEclipseCurvePoint(pbe, pointInitial, 1, 0.8);

                // 16:06:10 UT, convert to DT!
                double expectedTime = (16 + 06 / 60.0 + 10 / 3600.0 + pbe.DeltaT / 3600.0) / 24.0;

                Assert.AreEqual(35.4645, pointFinal.Latitude, epsCoord);
                Assert.AreEqual(expectedTime, new Date(pointFinal.JulianDay).Time, epsTime);
            }

            // Excercise from page 24 (3)
            {
                var pointInitial = new CrdsGeographical(110, 0);
                var pointFinal = SolarEclipses.FindEclipseCurvePoint(pbe, pointInitial, 1, 0.8);

                // 16:17:19 UT, convert to DT!
                double expectedTime = (16 + 17 / 60.0 + 19 / 3600.0 + pbe.DeltaT / 3600.0) / 24.0;

                Assert.AreEqual(38.1538, pointFinal.Latitude, epsCoord);
                Assert.AreEqual(expectedTime, new Date(pointFinal.JulianDay).Time, epsTime);
            }

            // Excercise from page 24 (4)
            {
                var pointInitial = new CrdsGeographical(105, 0);
                var pointFinal = SolarEclipses.FindEclipseCurvePoint(pbe, pointInitial, 1, 0.8);

                // 16:29:14 UT, convert to DT!
                double expectedTime = (16 + 29 / 60.0 + 14 / 3600.0 + pbe.DeltaT / 3600.0) / 24.0;

                Assert.AreEqual(40.7021, pointFinal.Latitude, epsCoord);
                Assert.AreEqual(expectedTime, new Date(pointFinal.JulianDay).Time, epsTime);
            }

            // Excercise from page 24 (5)
            {
                var pointInitial = new CrdsGeographical(100, 0);
                var pointFinal = SolarEclipses.FindEclipseCurvePoint(pbe, pointInitial, 1, 0.8);

                // 16:41:25 UT, convert to DT!
                double expectedTime = (16 + 41 / 60.0 + 25 / 3600.0 + pbe.DeltaT / 3600.0) / 24.0;

                Assert.AreEqual(43.0162, pointFinal.Latitude, epsCoord);
                Assert.AreEqual(expectedTime, new Date(pointFinal.JulianDay).Time, epsTime);
            }
        }

        /// <summary>
        /// Example 8 (EOSE)
        /// </summary>
        [TestMethod]
        public void LocalCircumstances()
        {
            // 10 may 1994
            var pbe = new PolynomialBesselianElements()
            {
                JulianDay0 = 2449483.20833,
                X = new[] { -0.173_367, +0.499_0629, +0.000_0296, -0.000_005_63 },
                Y = new[] { +0.383_484, +0.086_9393, -0.000_1183, -0.000_000_92 },
                D = new[] { +17.686_13, +0.010_642, -0.000_004, 0 },
                Mu = new[] { 75.909_23, 15.001_621, 0, 0 },
                L1 = new[] { +0.566_906, -0.000_0318, -0.000_0098, 0 },
                L2 = new[] { +0.020_679, -0.000_0317, -0.000_0097, 0 },
                TanF1 = 0.004_6308,
                TanF2 = 0.004_6077,
                Step = 1.0 / 24.0,
                DeltaT = 61 // Used in examples from Meeus' book
            };

            // 1 second of time is enough
            double epsTime = TimeSpan.FromSeconds(1).TotalDays;

            CrdsGeographical g = new CrdsGeographical(new DMS("+77*03'56''"), new DMS("+38*55'17''"));

            var local = SolarEclipses.LocalCircumstances(pbe, g);

            double jdPartialBegin = new Date(new DateTime(1994, 5, 10, 15, 39, 19, DateTimeKind.Utc)).ToJulianEphemerisDay();
            double jdMaxExpected = new Date(new DateTime(1994, 5, 10, 17, 26, 45, DateTimeKind.Utc)).ToJulianEphemerisDay();
            double jdPartialEnd = new Date(new DateTime(1994, 5, 10, 19, 13, 53, DateTimeKind.Utc)).ToJulianEphemerisDay();

            Assert.AreEqual(jdMaxExpected, local.JulianDayMax, epsTime);
            Assert.AreEqual(jdPartialBegin, local.JulianDayPartialBegin, epsTime);
            Assert.AreEqual(jdPartialEnd, local.JulianDayPartialEnd, epsTime);

            Assert.AreEqual(0.857, local.MaxMagnitude, 1e-4);
            Assert.AreEqual(0.9434, local.MoonToSunDiameterRatio, 1e-5);
        }

        [TestMethod]
        public void FindFunctionEnd()
        {
            {
                Func<double, bool> func = (x) => x < 2;
                var x0 = SolarEclipses.FindFunctionEnd(func, -5, 6, true, false, 1e-6);
                Assert.AreEqual(2, x0, 1e-6);
            }

            {
                Func<double, bool> func = (x) => x > 4;
                var x0 = SolarEclipses.FindFunctionEnd(func, -5, 5, false, true, 1e-6);
                Assert.AreEqual(4, x0, 1e-6);
            }
        }

        [TestMethod]
        public void SarosNumber()
        {
            // Solar eclipse 11 Aug 1999
            {
                double jd = 2451401;
                int saros = SolarEclipses.SarosNumber(jd);
                Assert.AreEqual(145, saros);
            }

            // Solar eclipse 3 Nov 2013
            {
                double jd = Date.JulianDay(new Date(2013, 11, 3));
                int saros = SolarEclipses.SarosNumber(jd);
                Assert.AreEqual(143, saros);
            }

            // Solar eclipse 22 Jul 2009
            {
                double jd = Date.JulianDay(new Date(2009, 7, 22));
                int saros = SolarEclipses.SarosNumber(jd);
                Assert.AreEqual(136, saros);
            }

            // Solar eclipse 30 Jul 1505
            {
                double jd = Date.JulianDay(new Date(1505, 7, 30));
                int saros = SolarEclipses.SarosNumber(jd);
                Assert.AreEqual(108, saros);
            }

            // Lunar eclipse 18 Aug 2016
            {
                double jd = Date.JulianDay(new Date(2016, 8, 18));
                int saros = LunarEclipses.Saros(jd);
                Assert.AreEqual(109, saros);
            }

            // Lunar eclipse 16 Sep 2016
            {
                double jd = Date.JulianDay(new Date(2016, 9, 16));
                int saros = LunarEclipses.Saros(jd);
                Assert.AreEqual(147, saros);
            }

            // Lunar eclipse 30 Oct 2031
            {
                double jd = Date.JulianDay(new Date(2031, 10, 30));
                int saros = LunarEclipses.Saros(jd);
                Assert.AreEqual(117, saros);
            }

            // Lunar eclipse 15 Oct 1521
            {
                double jd = Date.JulianDay(new Date(1521, 10, 15));
                int saros = LunarEclipses.Saros(jd);
                Assert.AreEqual(138, saros);
            }
        }
    }
}
