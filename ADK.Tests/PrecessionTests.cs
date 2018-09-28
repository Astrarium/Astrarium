using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADK.Tests
{
    [TestClass]
    public class PrecessionTests
    {
        [TestMethod]
        public void GetEquatorialCoordinatesOfEpoch()
        {
            // Equatorial coordinates of Theta Persei
            CrdsEquatorial eq0 = new CrdsEquatorial(
                new HMS(2, 44, 11.986).ToDecimalAngle(), 
                new DMS(49, 13, 42.48).ToDecimalAngle()
            );

            // proper motion of Theta Persei, units per year
            CrdsEquatorial pm = new CrdsEquatorial(
                new HMS(0, 0, 0.03425).ToDecimalAngle(),
                -new DMS(0, 0, 0.0895).ToDecimalAngle()
            );
            
            // target date (2018 November 13.19)
            double jd = Date.JulianDay(new Date(2028, 11, 13.19));
            Assert.AreEqual(2462088.69, jd);

            // years since initial epoch
            double years = (jd - Date.EPOCH_J2000) / 365.25;

            // Take into account effect of proper motion:
            // now coordinates are for the mean equinox of J2000.0,
            // but for epoch of the target date
            eq0.Alpha += pm.Alpha * years; 
            eq0.Delta += pm.Delta * years;

            // precessional elements
            var p = Precession.ElementsFK5(Date.EPOCH_J2000, jd);

            // Equatorial coordinates for the mean equinox and epoch
            // of the target date
            CrdsEquatorial eq = Precession.GetEquatorialCoordinatesOfEpoch(eq0, p);

            // Check final results
            Assert.AreEqual(new HMS(2, 46, 11.331), new HMS(eq.Alpha));
            Assert.AreEqual(new DMS(49, 20, 54.54), new DMS(eq.Delta));
        }
    }
}
