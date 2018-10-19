using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADK.Tests
{
    [TestClass]
    public class CoordinatesTests
    {
        [TestMethod]
        public void EquatorialToHorizontal()
        {
            // Equatorial coordinates of Venus
            var eq = new CrdsEquatorial(new HMS("23h 09m 16.641s"), new DMS("-6* 43' 11.61''"));

            // Geographical coordinates of US Naval Observatory at Washington, DC
            var geo = new CrdsGeographical(new DMS("+38* 55' 17''"), new DMS("+77* 03' 56''"));

            // Date of observation
            var date = new Date(new DateTime(1987, 4, 10, 19, 21, 0, DateTimeKind.Utc));

            // Mean sidereal time at Greenwich
            var theta0 = date.MeanSiderealTime();
            Assert.AreEqual(new HMS("8h 34m 57.0896s"), new HMS(theta0));

            // Apparent sidereal time at Greenwich

            // TODO: not correct!
            theta0 += (-3.868 / 15 * Math.Cos(AstroUtils.ToRadian(new DMS("23* 26' 36.87''").ToDecimalAngle()))) / 3600.0;
            Assert.AreEqual(new HMS("8h 34m 56.853s"), new HMS(theta0));

            // Expected local horizontal coordinates of Venus
            var hor = eq.ToHorizontal(geo, theta0);

            Assert.AreEqual(15.1249, hor.Altitude, 1e-4);
            Assert.AreEqual(68.0337, hor.Azimuth, 1e-4);

        }
    }
}
