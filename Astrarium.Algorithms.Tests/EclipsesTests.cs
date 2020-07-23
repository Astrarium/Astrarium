using Astrarium.Algorithms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
