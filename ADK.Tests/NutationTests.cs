using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADK.Tests
{
    [TestClass]
    public class NutationTests
    {
        double jd = Date.JulianDay(1987, 4, 10);
       
        /// <summary>
        /// AA(II), example 22.a.
        /// </summary>
        [TestMethod]
        public void NutationElements()
        {
            var nutation = Nutation.NutationElements(jd);

            var deltaPsi = nutation.deltaPsi * 3600;
            var deltaEpsilon = nutation.deltaEpsilon * 3600;

            Assert.AreEqual(-3.788, deltaPsi, 0.5);
            Assert.AreEqual(9.443, deltaEpsilon, 0.1);
        }

        /// <summary>
        /// AA(II), example 23.a.
        /// </summary>
        [TestMethod]
        public void NutationEffect()
        {
            CrdsEquatorial eq = new CrdsEquatorial(41.5472, 49.3485);

            NutationElements ne = new NutationElements()
            {
                deltaPsi = 14.861 / 3600,
                deltaEpsilon = 2.705 / 3600
            };

            double epsilon = 23.436;

            CrdsEquatorial correction = Nutation.NutationEffect(eq, ne, epsilon);

            Assert.AreEqual(15.843, correction.Alpha * 3600, 1e-3);
            Assert.AreEqual(6.218, correction.Delta * 3600, 1e-3);
        }
    }
}
