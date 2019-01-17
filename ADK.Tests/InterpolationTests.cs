using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADK.Tests
{
    [TestClass]
    public class InterpolationTests
    {

        [TestMethod]
        public void Lagrange()
        {
            double[] x = new double[] { 0, 1, 2, 5 };
            double[] y = new double[] { 2, 3, 12, 147 };

            double y0 = Interpolation.Lagrange(x, y, 3);

            Assert.AreEqual(35, y0);
        }

        
    }
}
