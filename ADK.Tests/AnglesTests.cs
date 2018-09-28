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
            Assert.AreEqual(new DMS(24, 27, 23.63), new DMS(24.456565));
            Assert.AreEqual(-new DMS(24, 27, 23.63), new DMS(-24.456565));           
            Assert.AreEqual(new DMS(14, 7, 27.57), new DMS(14.12432545435));
        }

        [TestMethod]
        public void FromDmsToDecimal()
        {
            // max error is 1/100 of second of arc
            double error = 1 / 3600.0 / 100;

            Assert.AreEqual(24.456565, new DMS(24, 27, 23.63).ToDecimalAngle(), error);
            Assert.AreEqual(-24.456565, -new DMS(24, 27, 23.63).ToDecimalAngle(), error);
            Assert.AreEqual(14.12432545435, new DMS(14, 7, 27.57).ToDecimalAngle(), error);
        }

        [TestMethod]
        public void FromDecimalToHms()
        {
            Assert.AreEqual(new HMS(4, 27, 40.386), new HMS(66.918277));
        }

        [TestMethod]
        public void ParseDMS()
        {
            Assert.AreEqual(new DMS(4, 27, 40.386), DMS.Parse("4* 27' 40.386"));
            Assert.AreEqual(new DMS(4, 27, 40.386), DMS.Parse("4*27'40.386''"));
            Assert.AreEqual(-new DMS(4, 27, 40.386), DMS.Parse("-4* 27' 40.386''"));
            Assert.AreEqual(new DMS(4, 27, 40.386), DMS.Parse("+ 4* 27' 40.386''"));
            Assert.AreEqual(-new DMS(4, 27, 40.386), DMS.Parse("- 4 27 40.386"));
        }
    }
}
