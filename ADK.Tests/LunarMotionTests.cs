using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ADK.Tests
{
    [TestClass]
    public class LunarMotionTests
    {
        [TestMethod]
        public void Test()
        {
            double jd = 2448724.5;

            CrdsEcliptical eclExpected = new CrdsEcliptical(133.162655, -3.229126, 368409.7);

            CrdsEcliptical ecl = LunarMotion.GetCoordinates(jd);
            Assert.AreEqual(eclExpected.Lambda, ecl.Lambda, 1e-6);
            Assert.AreEqual(eclExpected.Beta, ecl.Beta, 1e-6);
            Assert.AreEqual(eclExpected.Distance, ecl.Distance, 1e-1);
        }
    }
}
