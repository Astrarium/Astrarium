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
                new TestData()
                {
                    // Jupiter
                    Planet = new CrdsEcliptical(289.7284978, -0.0083066, 5.754),
                    Satellite = new CrdsEcliptical(289.1380251, -0.1787516, 5.816),
                    JulianDay = 2458910.5,                    
                    Orbit = new GenericSatelliteOrbit()
                    {
                        jd = 2458910.5,
                        M = 173.3391578385199,
                        n = 1.4334454486197841,
                        e = 0.1445592845834921,
                        a = 0.076704623733999638,
                        i = 29.281956648008659,
                        w = 28.857802792063811,
                        Om = 44.243050305709588,
                        Pw = 139.38,
                        POm = 292.57
                    }
                },

                // Elara
                // 2020-Mar-03 00:00
                new TestData()
                {
                    Planet = new CrdsEcliptical(289.9104513, -0.0100286, 5.754),
                    Satellite = new CrdsEcliptical(290.5880675, -0.3618171),
                    JulianDay = 2458911.5,
                    Orbit = new GenericSatelliteOrbit()
                    {
                        jd = 2458911.5,
                        M = 77.3445191940261,
                        n = 1.3997313639410149,
                        e = 0.20484227354508239,
                        a = 0.07793140889266964,
                        i = 30.592442329213181,
                        w = 196.27845249534491,
                        Om = 90.810465090589958,
                        Pw = 128.07,
                        POm = 265.3
                    }
                }
            };

            foreach (var testData in testDatas)
            {
                CrdsEcliptical eclSatellite = GenericSatellite.Position(testData.JulianDay, testData.Orbit, testData.Planet);
                Assert.IsTrue(Angle.Separation(eclSatellite, testData.Satellite) < 8.0 / 3600.0);

                //Assert.AreEqual(eclSatellite.Lambda, testData.Satellite.Lambda, 10.0 / 3600);
                //Assert.AreEqual(eclSatellite.Beta, testData.Satellite.Beta, 10.0 / 3600);
                //Assert.AreEqual(eclSatellite.Distance, testData.Satellite.Distance, 1e-3);
            }
        }
    }
}
