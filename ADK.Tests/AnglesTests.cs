using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADK.Tests
{
    [TestClass]
    public class AnglesTests
    {
        [TestMethod]
        public void FromDecimalToDms()
        {
            Assert.AreEqual(new DmsAngle(24, 27, 23.63), new DmsAngle(24.456565));
            Assert.AreEqual(new DmsAngle(-24, 27, 23.63), new DmsAngle(-24.456565));           
            Assert.AreEqual(new DmsAngle(14, 7, 27.57), new DmsAngle(14.12432545435));
        }

        [TestMethod]
        public void FromDmsToDecimal()
        {
            // max error is 1/100 of second of arc
            double error = 1 / 3600.0 / 100;

            Assert.AreEqual(24.456565, new DmsAngle(24, 27, 23.63).ToDecimalAngle(), error);
            Assert.AreEqual(-24.456565, new DmsAngle(-24, 27, 23.63).ToDecimalAngle(), error);
            Assert.AreEqual(14.12432545435, new DmsAngle(14, 7, 27.57).ToDecimalAngle(), error);
        }

        [TestMethod]
        public void FromDecimalToHms()
        {
            Assert.AreEqual(new HmsAngle(4, 27, 40.386), new HmsAngle(66.918277));
        }
    }
}
