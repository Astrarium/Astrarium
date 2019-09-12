using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ADK.Tests
{
    [TestClass]
    public class GalileanMoonsTests
    {
        [TestMethod]
        public void Magnitude()
        {
            double m = GalileanMoons.Magnitude(5.234, 5.267, 0);

            Assert.AreEqual(0, m);
        }
    }
}
