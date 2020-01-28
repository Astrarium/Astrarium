using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ADK.Tests
{
    [TestClass]
    public class UranianMoonsTests : TestClassBase
    {
        [TestMethod]
        public void Positions()
        {
            double jd = 2458877.044502315;
            double de = -45.64;
            double ad = 3.7;
            double pa = 254.6;

            var pos = UranianMoons.Positions(jd, de, ad, pa);
        }
    }
}
