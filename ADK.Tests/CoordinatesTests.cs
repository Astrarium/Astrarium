using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADK.Tests
{
    [TestClass]
    public class CoordinatesTests
    {
        private const double errorInHMS = 1 / 3600.0 * 0.1 * 15; // 0.1 of second of time
        private const double errorInDMS = 1 / 3600.0 * 0.1;      // 0.1 of second of degree

        /// <remarks>
        /// AA(II), Example 13.b. 
        /// </remarks>
        [TestMethod]
        public void EquatorialToHorizontal()
        {
            // Apparent equatorial coordinates of Venus
            var eq = new CrdsEquatorial(new HMS("23h 09m 16.641s"), new DMS("-6* 43' 11.61''"));

            // Geographical coordinates of US Naval Observatory at Washington, DC
            var geo = new CrdsGeographical(new DMS("+77* 03' 56''"), new DMS("+38* 55' 17''"));

            // Date of observation
            var jd = new Date(new DateTime(1987, 4, 10, 19, 21, 0, DateTimeKind.Utc)).ToJulianDay();

            // Mean sidereal time at Greenwich
            var theta0 = Date.MeanSiderealTime(jd);
            Assert.AreEqual(new HMS("8h 34m 57.0896s"), new HMS(theta0));

            // Nutation elements
            var nutation = Nutation.NutationElements(jd);

            // True obliquity
            var epsilon = Date.TrueObliquity(jd, nutation.deltaEpsilon);

            // Apparent sidereal time at Greenwich
            theta0 = Date.ApparentSiderealTime(jd, nutation.deltaPsi, epsilon);
            Assert.AreEqual(new HMS("8h 34m 56.853s"), new HMS(theta0));

            // Expected local horizontal coordinates of Venus
            var hor = eq.ToHorizontal(geo, theta0);

            Assert.AreEqual(15.1249, hor.Altitude, 1e-4);
            Assert.AreEqual(68.0336, hor.Azimuth, 1e-4);
        }

        /// <remarks>
        /// Test based on AA(II), Example 13.b. 
        /// </remarks>
        [TestMethod]
        public void HorizontalToEquatorial()
        {
            // Apparent local horizontal coordinates of Venus
            var hor = new CrdsHorizontal(68.0337, 15.1249);

            // Geographical coordinates of US Naval Observatory at Washington, DC
            var geo = new CrdsGeographical(new DMS("+77* 03' 56''"), new DMS("+38* 55' 17''"));

            // Date of observation
            var jd = new Date(new DateTime(1987, 4, 10, 19, 21, 0, DateTimeKind.Utc)).ToJulianDay();

            // Nutation elements
            var nutation = Nutation.NutationElements(jd);

            // True obliquity
            var epsilon = Date.TrueObliquity(jd, nutation.deltaEpsilon);

            // Apparent sidereal time at Greenwich
            var theta0 = Date.ApparentSiderealTime(jd, nutation.deltaPsi, epsilon);
            Assert.AreEqual(new HMS("8h 34m 56.853s"), new HMS(theta0));

            // Expected apparent equatorial coordinates of Venus
            var eqExpected = new CrdsEquatorial(new HMS("23h 09m 16.641s"), new DMS("-6* 43' 11.61''"));

            // Tested value
            var eqActual = hor.ToEquatorial(geo, theta0);

            Assert.AreEqual(eqExpected.Alpha, eqActual.Alpha, errorInHMS);
            Assert.AreEqual(eqExpected.Delta, eqActual.Delta, errorInDMS);
        }

        /// <remarks>
        /// AA(II), Example 13.a. 
        /// </remarks>
        [TestMethod]
        public void EquatorialToEcliptical()
        {
            CrdsEquatorial eq = new CrdsEquatorial(new HMS("7h 45m 18.946s"), new DMS("+28* 01' 34.26''"));

            CrdsEcliptical ecl = eq.ToEcliptical(23.4392911);

            Assert.AreEqual(113.215630, ecl.Lambda, errorInDMS);
            Assert.AreEqual(6.684170, ecl.Beta, errorInDMS);
        }

        /// <remarks>
        /// AA(II), Example 13.a (exercise).
        /// </remarks>
        [TestMethod]
        public void EclipticalToEquatorial()
        {
            CrdsEcliptical ecl = new CrdsEcliptical(113.215630, 6.684170);

            CrdsEquatorial eq = ecl.ToEquatorial(23.4392911);

            CrdsEquatorial eqExpected = new CrdsEquatorial(new HMS("7h 45m 18.946s"), new DMS("+28* 01' 34.26''"));

            Assert.AreEqual(eqExpected.Alpha, eq.Alpha, errorInHMS);
            Assert.AreEqual(eqExpected.Delta, eq.Delta, errorInDMS);
        }

        /// <remarks>
        /// AA(II), page 96, exercise.
        /// </remarks>
        [TestMethod]
        public void EquatorialToGalactical()
        {
            CrdsEquatorial eq = new CrdsEquatorial(new HMS("17h 48m 59.74s"), new DMS("-14* 43' 08.2''"));

            CrdsGalactical gal = eq.ToGalactical();

            Assert.AreEqual(12.9593, gal.l, 1e-4);
            Assert.AreEqual(6.0463, gal.b, 1e-4);
        }

        /// <remarks>
        /// AA(II), page 96, exercise (inverted).
        /// </remarks>
        [TestMethod]
        public void GalacticalToEquatorial()
        {
            CrdsEquatorial eqExpected = new CrdsEquatorial(new HMS("17h 48m 59.74s"), new DMS("-14* 43' 08.2''"));

            CrdsGalactical gal = new CrdsGalactical(12.9593, 6.0463);

            CrdsEquatorial eq = gal.ToEquatorial();

            Assert.AreEqual(eqExpected.Alpha, eq.Alpha, 1e-4);
            Assert.AreEqual(eqExpected.Delta, eq.Delta, 1e-4);
        }

        /// <remarks>
        /// AA(II), example 26.a.
        /// </remarks>
        [TestMethod]
        public void EclipticalToRectangular()
        {
            CrdsEcliptical ecl = new CrdsEcliptical(199.907347, 0.62 / 3600.0, 0.99760775);

            CrdsRectangular rect = ecl.ToRectangular(23.4402297);

            Assert.AreEqual(-0.9379952, rect.X, 1e-7);
            Assert.AreEqual(-0.3116544, rect.Y, 1e-7);
            Assert.AreEqual(-0.1351215, rect.Z, 1e-7);
        }

        /// <summary>
        /// Example 40.a
        /// </summary>
        [TestMethod]
        public void ToTopocentric()
        {
            // Geocentric coordinates of Mars
            CrdsEquatorial eq = new CrdsEquatorial(339.530208, -15.771083);

            // Palomar Observatory coordinates, see example 11.a
            CrdsGeographical geo = new CrdsGeographical(new HMS("7h 47m 27s").ToDecimalAngle(), new DMS("+33* 21' 22''").ToDecimalAngle(), 1706);

            // Equatoria horizontal parallax 
            double pi = 23.592 / 3600;

            // Apparent sidereal time at Greenwich
            double theta0 = new HMS("1h 40m 45s").ToDecimalAngle();

            // Equatorial topocentric coordinates of Mars
            CrdsEquatorial topo = eq.ToTopocentric(geo, theta0, pi);

            Assert.AreEqual(new HMS("22h 38m 08.54s").ToDecimalAngle(), topo.Alpha, errorInHMS);
            Assert.AreEqual(new DMS("-15* 46' 30.04''").ToDecimalAngle(), topo.Delta, errorInDMS);
        }
    }
}
