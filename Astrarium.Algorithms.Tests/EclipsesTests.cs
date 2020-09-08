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

            var elements = SolarEclipses.FindInstantBesselianElements(position);

            Assert.AreEqual(15.32734, elements.D, 1e-2);
            Assert.AreEqual(343.68741, elements.Mu, 1e-1);
            Assert.AreEqual(0.070042, elements.X, 1e-2);
            Assert.AreEqual(0.502841, elements.Y, 1e-2);
            Assert.AreEqual(0.542469, elements.L1, 1e-2);
            Assert.AreEqual(-0.003650, elements.L2, 1e-2);
        }

        [TestMethod]
        public void FindNearestSolarEclipse()
        {
            double jd = new Date(1993, 5, 21).ToJulianDay();
            SolarEclipse eclipse = SolarEclipses.NearestEclipse(jd, true);

            Assert.AreEqual(2449129.0979, eclipse.JulianDayMaximum, TimeSpan.FromMinutes(0.36).TotalDays);
            Assert.AreEqual(SolarEclipseType.Partial, eclipse.EclipseType);
            Assert.AreEqual(0.740, eclipse.Phase, 1e-3);
            Assert.AreEqual(EclipseRegio.Northern, eclipse.Regio);
        }

        [TestMethod]
        public void ProjectionTests()
        {
            {
                double d = 15.32558;
                double mu = 345.88554;
                CrdsGeographical g = new CrdsGeographical(-26.71875, 44.66865);
                Vector v = SolarEclipses.ProjectOnFundamentalPlane(g, d, mu);
                CrdsGeographical g0 = SolarEclipses.ProjectOnEarth(new System.Drawing.PointF((float)v.X, (float)v.Y), d, mu);
                Assert.AreEqual(g.Latitude, g0.Latitude, 1e-5);
                Assert.AreEqual(g.Longitude, g0.Longitude, 1e-5);
            }

            {
                double d = -23.25608;
                double mu = 48.57292;
                CrdsGeographical g = new CrdsGeographical(97.20703, -28.22697);
                Vector v = SolarEclipses.ProjectOnFundamentalPlane(g, d, mu);
                CrdsGeographical g0 = SolarEclipses.ProjectOnEarth(new System.Drawing.PointF((float)v.X, (float)v.Y), d, mu);
                Assert.AreEqual(g.Latitude, g0.Latitude, 1e-5);
                Assert.AreEqual(g.Longitude, g0.Longitude, 1e-5);
            }
        }
    }
}
