using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ADK.Tests
{
    [TestClass]
    public class GenericSatelliteTests : TestClassBase
    {
        private class TestData
        {
            public double JulianDay { get; set; }
            public CrdsEcliptical Satellite { get; set; }
            public CrdsEcliptical Planet { get; set; }
            public GenericSatelliteOrbit Orbit { get; set; }
        }

        [TestMethod]
        public void Positions()
        {
            // Test data is obtained from NASA JPL Ephemeris tool
            // https://ssd.jpl.nasa.gov/horizons.cgi

            List<TestData> testDatas = new List<TestData>()
            {
                // Himalia
                // 2020-Mar-03 00:00 
                new TestData()
                {
                    // Jupiter
                    Planet = new CrdsEcliptical(289.7284978, -0.0083066, 5.754),
                    Satellite = new CrdsEcliptical(289.1380251, -0.1787516, 5.816),
                    JulianDay = 2458910.5,                    
                    Orbit = new GenericSatelliteOrbit()
                    {
                        jd0 = 2458910.5,
                        M0 = 173.3391578385199,
                        n = 1.4334454486197841,
                        e = 0.1445592845834921,
                        a = 0.076704623733999638,
                        i = 29.281956648008659,
                        omega0 = 28.857802792063811,
                        node0 = 44.243050305709588,
                        Pw = 139.38,
                        Pnode = 292.57
                    }
                },
            };

            foreach (var testData in testDatas)
            {
                CrdsEcliptical eclSatellite = GenericSatellite.Position(testData.JulianDay, testData.Orbit, testData.Planet);
                Assert.AreEqual(eclSatellite.Lambda, testData.Satellite.Lambda, 1.0 / 3600);
                Assert.AreEqual(eclSatellite.Beta, testData.Satellite.Beta, 4.0 / 3600);
                Assert.AreEqual(eclSatellite.Distance, testData.Satellite.Distance, 1e-3);
            }
        }
    }
}
