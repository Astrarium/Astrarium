using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADK.Tests
{
    [TestClass]
    public class MinorBodyPositionsTests
    {
        [TestMethod]
        public void Reduction()
        {
            var oe0 = new OrbitalElements()
            {
                i = 47.1220,
                omega = 151.4486,
                Omega = 45.7481
            };

            var oe = MinorBodyPositions.Reduction(oe0, 2358042.5305, 2433282.4235);

            Assert.AreEqual(47.1378, oe.i, 1e-3);
            Assert.AreEqual(151.4783, oe.omega, 1e-3);
            Assert.AreEqual(48.6030, oe.Omega, 1e-3);
        }        
    }
}
