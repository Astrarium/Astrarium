using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADK.Tests
{
    [TestClass]
    public class AberrationTests
    {
        [TestMethod]
        public void AberrationElements()
        {
            // AA(II) example 25.a
            {
                AberrationElements ae = Aberration.AberrationElements(2448908.5);
                Assert.AreEqual(0.016711668, ae.e, 1e-9);
                Assert.AreEqual(199.90988, ae.lambda, 1e-5);
            }
             
            // AA(II) example 23.a
            {
                AberrationElements ae = Aberration.AberrationElements(Date.JulianDay(2028, 11, 13.19));
                Assert.AreEqual(0.01669649, ae.e, 1e-8);
                Assert.AreEqual(231.328, ae.lambda, 1e-3);
                Assert.AreEqual(103.434, ae.pi, 1e-3);
            }
        }
    }
}
