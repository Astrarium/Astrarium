using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADK.Tests
{
    [TestClass]
    public class SolarEphemTests
    {
        /// <summary>
        /// Synthetic tests, solar semidiameter at 1 au, aphelion and perihelion.
        /// </summary>
        [TestMethod]
        public void Semidiameter()
        {
            Assert.AreEqual(959.63, SolarEphem.Semidiameter(1), 1e-2);
            Assert.AreEqual(943.87, SolarEphem.Semidiameter(1.0167), 1e-2);
            Assert.AreEqual(975.94, SolarEphem.Semidiameter(0.98329), 1e-2);
        }

        /// <summary>
        /// Synthetic tests, solar parallax in arc seconds at 1 au, aphelion and perihelion.
        /// </summary>
        [TestMethod]
        public void Parallax()
        {
            Assert.AreEqual(8.794, SolarEphem.Parallax(1) * 3600, 1e-3);
            Assert.AreEqual(8.650, SolarEphem.Parallax(1.0167) * 3600, 1e-3);
            Assert.AreEqual(8.943, SolarEphem.Parallax(0.98329) * 3600, 1e-3);
        }
    }
}
