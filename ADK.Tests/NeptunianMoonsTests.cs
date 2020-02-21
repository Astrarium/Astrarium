using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ADK.Tests
{
    [TestClass]
    public class NeptunianMoonsTests : TestClassBase
    {
        [TestMethod]
        public void Positions()
        {
            // 2020-Feb-19 15:00  UTC
            //double jd = 2458899.125000000;

            // 1952 dec 19
            double jd = 2434366.13688;

            // expected coodinates of Triton
            double X = 10.589;
            double Y = -0.167;

            // position of Neptune, j2000
            double alpha = 348.857;
            double delta = -5.892;

            double pa = 90.905;   // position angle
            double sep = 10.5908; // expected separation, arcsec

            CrdsEquatorial eqNeptune = new CrdsEquatorial(new HMS("13h 31m 58s"), new DMS("-7* 50' 28''"));

            var p = NeptunianMoons.Positions(jd, eqNeptune, 30.874);


            //Assert.AreEqual(X, p.X, 1e-2);
            //Assert.AreEqual(Y, p.Y, 1e-2);

            //Assert.AreEqual(0, p.Item1);
        }
    }
}
