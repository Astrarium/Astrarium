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
