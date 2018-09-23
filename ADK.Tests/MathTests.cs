using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADK.Tests
{
    [TestClass]
    public class MathTests
    {
        [TestMethod]
        public void To360()
        {
            Assert.AreEqual(2, AstroUtils.To360(362));
            Assert.AreEqual(183, AstroUtils.To360(183));
            Assert.AreEqual(348, AstroUtils.To360(-12));
            Assert.AreEqual(90, AstroUtils.To360(-270));
        }
    }
}
