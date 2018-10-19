﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADK.Tests
{
    [TestClass]
    public class NutationTests
    {
        double jd = Date.JulianDay(1987, 4, 10);

        [TestMethod]
        public void MeanObliquity()
        {
            var epsilon0 = Nutation.MeanObliquity(jd);
            Assert.AreEqual(new DMS("23* 26' 27.407''"), new DMS(epsilon0));
        }

        [TestMethod]
        public void TrueObliquity()
        {
            var epsilon = Nutation.TrueObliquity(jd);
            Assert.AreEqual(new DMS("23* 26' 36.850''").ToDecimalAngle(), epsilon, 1 / 3600.0 / 2);
        }

        [TestMethod]
        public void NutationInLongitude()
        {
            var deltaPsi = Nutation.NutationInLongitude(jd) * 3600;
            Assert.AreEqual(-3.788, deltaPsi, 0.5);
        }

        [TestMethod]
        public void NutationInObliquity()
        {
            var deltaEpsilon = Nutation.NutationInObliquity(jd) * 3600;
            Assert.AreEqual(9.443, deltaEpsilon, 0.5);
        }
    }
}
