using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Astrarium.Algorithms.Tests
{
    [TestClass]
    public class LunarMotionTests
    {
        /// <summary>
        /// AA(II), example 47.a
        /// </summary>
        [TestMethod]
        public void GetCoordinates()
        {
            double jd = 2448724.5;

            // geocentrical coordinates
            CrdsEcliptical ecl = LunarMotion.GetCoordinates(jd);
            Assert.AreEqual(133.162655, ecl.Lambda, 1e-6);
            Assert.AreEqual(-3.229126, ecl.Beta, 1e-6);
            Assert.AreEqual(368409.7, ecl.Distance, 1e-1);

            // get nutation elements
            var nutation = Nutation.NutationElements(jd);

            // apparent geocentrical ecliptical coordinates 
            ecl += Nutation.NutationEffect(nutation.deltaPsi);

            // true obliquity of the Earth orbit
            double epsilon = Date.TrueObliquity(jd, nutation.deltaEpsilon);

            // equatorial geocentrical coordinates
            CrdsEquatorial eq = ecl.ToEquatorial(epsilon);

            // Max error in Right Ascention is 0.1" of time
            double errAlpha = new HMS("0h 0m 00.1s").ToDecimalAngle();

            // Max error in Declination is 1" of arc
            double errDelta = new DMS("0* 0' 01''").ToDecimalAngle();

            Assert.AreEqual(new HMS("8h 58m 45.2s").ToDecimalAngle(), eq.Alpha, errAlpha);
            Assert.AreEqual(new DMS("+13* 46' 06''").ToDecimalAngle(), eq.Delta, errDelta);
        }
    }
}
