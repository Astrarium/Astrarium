using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Astrarium.Algorithms.Tests
{
    [TestClass]
    public class ConstellationsTests : TestClassBase
    {
        private static List<TestData> testData = new List<TestData>();

        [TestMethod]
        public void FindConstellation()
        {
            // precessional elements for converting from J1950 to B1875 epoch
            var p = Precession.ElementsFK5(Date.EPOCH_J1950, Date.EPOCH_B1875);

            foreach (var test in testData)
            {
                // Equatorial coordinates for B1875 epoch
                CrdsEquatorial eq = Precession.GetEquatorialCoordinates(new CrdsEquatorial(test.RA, test.Dec), p);

                // Constellation name
                string con = Constellations.FindConstellation(eq);

                // Check result
                Assert.AreEqual(test.ConstName, con);
            }
        }

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var lines = ReadLinesFromResource("Astrarium.Algorithms.Tests.Data.Constells.chk", Encoding.ASCII).ToList();

            foreach (string line in lines)
            {
                double ra = double.Parse(line.Substring(0, 8), CultureInfo.InvariantCulture);
                double dec = double.Parse(line.Substring(8, 8), CultureInfo.InvariantCulture);
                string con = line.Substring(17, 3);

                testData.Add(new TestData(ra, dec, con));
            }
        }

        private struct TestData
        {
            public double RA { get; set; }
            public double Dec { get; set; }
            public string ConstName { get; set; }

            public TestData(double ra, double dec, string constName)
            {
                RA = ra;
                Dec = dec;
                ConstName = constName;
            }
        }
    }
}
