using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Astrarium.Algorithms.Tests
{
    [TestClass]
    public class GalileanMoonsTests : TestClassBase
    {
        [TestMethod]
        public void Positions()
        {
            var earth = new CrdsHeliocentrical()
            {
                L = 164.39745364456394,
                B = -0.00010322443896298875,
                R = 0.9917003651870131
            };

            var jupiter = new CrdsHeliocentrical()
            {
                L = 312.39309804246045,
                B = -0.68464503476063021,
                R = 5.074939001057106
            };

            for (int i = 0; i < 30 * 24; i++)
            {
                var pos = GalileanMoons.Positions(2459278.25976003, earth, jupiter);
            }
        }

        [TestMethod]
        public void Magnitude()
        {
            var lines = ReadLinesFromResource("Astrarium.Algorithms.Tests.Data.GalileanMoonsMag.chk", Encoding.ASCII).ToArray();
         
            foreach (string line in lines)
            {
                string[] testRecordSplitted = line.Split(';');

                int moonIndex = int.Parse(testRecordSplitted[0].Trim(), CultureInfo.InvariantCulture);
                double mag = double.Parse(testRecordSplitted[1].Trim(), CultureInfo.InvariantCulture);
                double phase = double.Parse(testRecordSplitted[2].Trim(), CultureInfo.InvariantCulture) / 100;
                double r = double.Parse(testRecordSplitted[3].Trim(), CultureInfo.InvariantCulture);
                double delta = double.Parse(testRecordSplitted[4].Trim(), CultureInfo.InvariantCulture);

                double m = GalileanMoons.Magnitude(r, delta, phase, moonIndex);
                Assert.AreEqual(mag, m, 0.2);
            }
        }
    }
}
