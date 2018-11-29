using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ADK.Tests
{
    [TestClass]
    public class PlanetEphemTests
    {
        /// <summary>
        /// AA(II), example 45.a
        /// </summary>
        [TestMethod]
        public void SaturnRings()
        {
            double jd = 2448972.5 + 0.00068;

            CrdsHeliocentrical earth = PlanetPositions.GetPlanetCoordinates(3, jd, true);
            CrdsHeliocentrical saturn = PlanetPositions.GetPlanetCoordinates(6, jd, true);
           
            RingsAppearance rings = PlanetEphem.SaturnRings(jd, saturn, earth, 23.43971);

            Assert.AreEqual(35.87, rings.a, 1e-2);
            Assert.AreEqual(10.15, rings.b, 1e-2);
            Assert.AreEqual(16.442, rings.B, 1e-3);
            Assert.AreEqual(4.198, rings.DeltaU, 1e-3);
            Assert.AreEqual(6.741, rings.P, 1e-3);
        }

        /// <summary>
        /// Example from book "Practical Ephemeris Calculations" (Montenbruck), page 93.
        /// </summary>
        [TestMethod]
        public void PlanetAppearance()
        {
            // 1 January 1982, TD
            double jd = 2444970.500608;

            // Geocentric coordinates of Mars
            CrdsEquatorial eq = new CrdsEquatorial(187.4042, -0.6522);
 
            // 0 in this case means no light-time effect
            PlanetAppearance a = PlanetEphem.PlanetAppearance(jd, 4, eq, 0);

            Assert.AreEqual(30.24, a.P, 1e-2);
            Assert.AreEqual(23.50, a.D, 1e-2);
            Assert.AreEqual(150.97, a.CM, 1e-2);

            // with respect of light-time effect
            double distance = 1.290617;
            double cm = Math.Abs(PlanetEphem.PlanetAppearance(jd, 4, eq, distance).CM - a.CM);

            // difference in longitude shoould be about 2.62 (see page 96).
            Assert.AreEqual(2.62, cm, 1e-2);
        }
    }
}
