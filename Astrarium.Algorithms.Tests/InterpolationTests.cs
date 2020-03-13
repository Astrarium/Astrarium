using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Astrarium.Algorithms.Tests
{
    [TestClass]
    public class InterpolationTests
    {
        [TestMethod]
        public void Lagrange()
        {
            {
                double[] x = new double[] { 0, 1, 2, 5 };
                double[] y = new double[] { 2, 3, 12, 147 };
                double y0 = Interpolation.Lagrange(x, y, 3);
                Assert.AreEqual(35, y0);
            }
            {
                double[] x = new double[] { 2, 3, 5, 8, 12 };
                double[] y = new double[] { 10, 15, 25, 40, 60 };
                double y0 = Interpolation.Lagrange(x, y, 4);
                Assert.AreEqual(20, y0);
            }
            {
                double[] x = new double[] {1951, 1961, 1971 };
                double[] y = new double[] { 2.8, 3.2, 4.5 };
                double y0 = Interpolation.Lagrange(x, y, 1966);
                Assert.AreEqual(3.7375, y0, 1e-4);
            }
        }        
    }
}
