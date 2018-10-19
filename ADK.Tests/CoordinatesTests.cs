using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADK.Tests
{
    [TestClass]
    public class CoordinatesTests
    {
        /// <remarks>
        /// AA(II), Example 13.b. 
        /// </remarks>
        [TestMethod]
        public void EquatorialToHorizontal()
        {
            // Apparent equatorial coordinates of Venus
            var eq = new CrdsEquatorial(new HMS("23h 09m 16.641s"), new DMS("-6* 43' 11.61''"));

            // Geographical coordinates of US Naval Observatory at Washington, DC
            var geo = new CrdsGeographical(new DMS("+38* 55' 17''"), new DMS("+77* 03' 56''"));

            // Date of observation
            var jd = new Date(new DateTime(1987, 4, 10, 19, 21, 0, DateTimeKind.Utc)).ToJulianDay();

            // Mean sidereal time at Greenwich
            var theta0 = Date.MeanSiderealTime(jd);
            Assert.AreEqual(new HMS("8h 34m 57.0896s"), new HMS(theta0));

            // Apparent sidereal time at Greenwich
            theta0 = Date.ApparentSiderealTime(jd);
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
            var geo = new CrdsGeographical(new DMS("+38* 55' 17''"), new DMS("+77* 03' 56''"));

            // Date of observation
            var jd = new Date(new DateTime(1987, 4, 10, 19, 21, 0, DateTimeKind.Utc)).ToJulianDay();

            // Apparent sidereal time at Greenwich
            var theta0 = Date.ApparentSiderealTime(jd);
            Assert.AreEqual(new HMS("8h 34m 56.853s"), new HMS(theta0));

            // Expected apparent equatorial coordinates of Venus
            var eqExpected = new CrdsEquatorial(new HMS("23h 09m 16.641s"), new DMS("-6* 43' 11.61''"));
       
            // Tested value
            var eqActual = hor.ToEquatorial(geo, theta0);

            double errorInAlpha = 1 / 3600.0 * 15 * 0.1; // 0.1 of second of time
            double errorInDelta = 1 / 3600.0 * 0.1;      // 0.1 of second of degree

            Assert.AreEqual(eqExpected.Alpha, eqActual.Alpha, errorInAlpha);
            Assert.AreEqual(eqExpected.Delta, eqActual.Delta, errorInDelta);
        }
    }
}
