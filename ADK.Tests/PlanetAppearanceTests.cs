using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADK.Tests
{
    [TestClass]
    public class PlanetAppearanceTests
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
           
            RingsAppearance rings = PlanetAppearance.SaturnRings(jd, saturn, earth, 23.43971);

            Assert.AreEqual(35.87, rings.a, 1e-2);
            Assert.AreEqual(10.15, rings.b, 1e-2);
            Assert.AreEqual(16.442, rings.B, 1e-3);
            Assert.AreEqual(4.198, rings.DeltaU, 1e-3);
            Assert.AreEqual(6.741, rings.P, 1e-3);
        }
    }
}
