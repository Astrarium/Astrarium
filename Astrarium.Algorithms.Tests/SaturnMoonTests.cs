using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Astrarium.Algorithms.Tests
{
    [TestClass]
    public class SaturnMoonsTests : TestClassBase
    {
        [TestMethod]
        public void Positions()
        {
            var earth = new CrdsHeliocentrical()
            {
                L = 174.655278 + 180,
                B = -0.000228,
                R = 1.0050057
            };

            var saturn = new CrdsHeliocentrical()
            {
                L = 41.912356,
                B = -2.360096,
                R = 9.207193
            };

            double[] expected = new double[]
            {
                3.102, -0.204,
                3.823, 0.318,
                4.027, -1.061,
                -5.365, -1.148,
                -0.972, -3.137,
                14.568, 4.727,
                -18.004, -5.319,
                -48.760, 4.150
            };

            var pos = SaturnianMoons.Positions(2451439.50074, earth, saturn);

            for (int i = 0; i < 8; i++)
            {
                Assert.AreEqual(expected[2 * i], pos[i].X, 1e-3);
                Assert.AreEqual(expected[2 * i + 1], pos[i].Y, 1e-3);
            }
        }

    }
}
