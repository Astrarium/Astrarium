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
    public class NeptunianMoonsTests : TestClassBase
    {
        private class TestData
        {
            public int MoonNumber { get; set; }
            public double JulianDay { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public CrdsEcliptical Neptune { get; set; }
        }

        [TestMethod]
        public void Positions()
        {
            // Test data is obtained from NASA JPL Ephemeris tool
            // https://ssd.jpl.nasa.gov/horizons.cgi

            List<TestData> testDatas = new List<TestData>() 
            {
                // Triton
                // 2020-Jan-07 13:00 2458856.041666667    -0.964   12.797 
                new TestData() 
                {
                    MoonNumber = 1,
                    Neptune = new CrdsEcliptical(new DMS("346* 24' 16.12''"), new DMS("-1* 01' 26.26''"), 30.416),
                    JulianDay = 2458856.041666667,
                    X = -0.964,
                    Y = 12.797
                },

                // Triton
                // 2025-Jan-22 19:00 2460698.291666667        1.086  -12.382 
                new TestData()
                {
                    MoonNumber = 1,
                    Neptune = new CrdsEcliptical(new DMS("357° 43' 31.73''"), new DMS("-1° 16' 24.69''"), 30.416),
                    JulianDay = 2460698.291666667,
                    X = 1.086,
                    Y = -12.382
                },

                // Nereid
                // 2020-Feb-27 00:00 2458906.500000000        5.124   10.556 
                new TestData()
                {
                    MoonNumber = 2,
                    Neptune = new CrdsEcliptical(new DMS("348* 00' 11.97''"), new DMS("-1* 00' 54.27''"), 30.906),
                    JulianDay = 2458906.5,
                    X = 5.124,
                    Y = 10.556
                },

                // Nereid
                // 2020-Jul-13 00:00 2459043.500000000      384.491  206.269 
                new TestData()
                {
                    MoonNumber = 2,
                    Neptune = new CrdsEcliptical(new DMS("350* 51' 10.8''"), new DMS("-1* 05' 15.56''"), 29.405),
                    JulianDay = 2459043.5,
                    X = 384.491,
                    Y = 206.269
                },

                // Nereid
                // 1980-Jan-01 00:00 2444239.500000000      135.407   32.771
                new TestData()
                {
                    MoonNumber = 2,
                    Neptune = new CrdsEcliptical(new DMS("260* 55' 41.93''"), new DMS("+1* 20' 26.28''"), 31.209),
                    JulianDay = 2444239.5,
                    X =  135.407,
                    Y = 32.771
                }
            };

            // possible error in coordinates is 1 arcsecond
            const double error = 1; 

            foreach (var testData in testDatas)
            {
                NutationElements ne = Nutation.NutationElements(testData.JulianDay);
                double epsilon = Date.TrueObliquity(testData.JulianDay, ne.deltaEpsilon);
                CrdsEcliptical eclSatellite = NeptunianMoons.Position(testData.JulianDay, testData.Neptune, testData.MoonNumber);

                CrdsEquatorial eqNeptune = testData.Neptune.ToEquatorial(epsilon);
                CrdsEquatorial eqSatellite = eclSatellite.ToEquatorial(epsilon);

                double X = (eqSatellite.Alpha - eqNeptune.Alpha) * Math.Cos(Angle.ToRadians(eqNeptune.Delta)) * 3600;
                double Y = (eqSatellite.Delta - eqNeptune.Delta) * 3600;

                Assert.AreEqual(testData.X, X, error);
                Assert.AreEqual(testData.Y, Y, error);
            }
        }
    }
}
