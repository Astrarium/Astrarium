using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ADK.Tests
{
    [TestClass]
    public class ConstellationsTests : TestClassBase
    {
        private static List<TestData> testData = new List<TestData>();

        [TestMethod]
        public void GetConstellationByCoordinates()
        {
            foreach (var test in testData)
            {
                string con = Constellations.GetConstellationByCoordinates(new CrdsEquatorial(test.RA, test.Dec), Date.EPOCH_B1950);
                Assert.AreEqual(test.Name, con);
            }
        }

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var lines = ReadLinesFromResource("ADK.Tests.Data.Constells.chk", Encoding.ASCII).ToList();

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
            public string Name { get; set; }

            public TestData(double ra, double dec, string con)
            {
                RA = ra;
                Dec = dec;
                Name = con;
            }
        }
    }
}
