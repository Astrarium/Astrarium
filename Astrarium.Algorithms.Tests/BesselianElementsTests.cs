using Astrarium.Algorithms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Astrarium.Algorithms.Tests
{
    [TestClass]
    public class BesselianElementsTests
    {
        [TestMethod]
        public void BesselianElements()
        {
            // 11 Aug 1999, 11:00:00 TDT
            // Besselian elements are taken from
            // https://eclipse.gsfc.nasa.gov/SEsearch/SEdata.php?Ecl=+19990811
            // Sun and Moon geocentric positions are taken
            // from NASA horizons tool
            // https://ssd.jpl.nasa.gov/horizons.cgi

            // Sun and Moon geocentric coordinates
            var sun = new CrdsEquatorial(new HMS("09h 23m 11.27s"), new DMS("+15* 19' 30.1''"));
            var moon = new CrdsEquatorial(new HMS("09h 23m 30.71s"), new DMS("+15* 48' 50.3''"));

            // Julian day
            double jd = 2451401.958333333;
            
            // Distance to Sun, in Earth equatorial radii
            double rs = 1.5162934697E+08 / 6371.0;

            // Distance to Moon, in Earth equatorial radii
            double rm = 3.7330565613E+05 / 6371.0;

            var elements = Algorithms.BesselianElements.Calculate(jd, sun, moon, rs, rm);

            Assert.AreEqual(15.32734, elements.D, 1e-2);
            Assert.AreEqual(343.68741, elements.Mu, 1e-1);
            Assert.AreEqual(0.070042, elements.X, 1e-2);
            Assert.AreEqual(0.502841, elements.Y, 1e-2);
            Assert.AreEqual(0.542469, elements.L1, 1e-2);
            Assert.AreEqual(-0.003650, elements.L2, 1e-2);

            Algorithms.BesselianElements.MoonShadowCenter(jd, sun, moon, 1.5162934697E+08, 3.7330565613E+05);
        }
    }
}
