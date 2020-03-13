using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Linq;

namespace Astrarium.Algorithms.Tests
{
    [TestClass]
    public class AngleTests
    {
        [TestMethod]
        public void To360()
        {
            Assert.AreEqual(2, Angle.To360(362));
            Assert.AreEqual(183, Angle.To360(183));
            Assert.AreEqual(348, Angle.To360(-12));
            Assert.AreEqual(90, Angle.To360(-270));
            Assert.AreEqual(1, Angle.To360(-719));
        }

        [TestMethod]
        public void AngleRangeIntersections()
        {
            {
                var s1 = new AngleRange(314, 90);
                var s2 = new AngleRange(345, 110);
                var inters = s1.Overlaps(s2);

                Assert.AreEqual(1, inters.Count);
                Assert.AreEqual(59, inters.ElementAt(0).Range);
            }

            {
                var s1 = new AngleRange(0, 180);
                var s2 = new AngleRange(75, 30);
                var inters = s1.Overlaps(s2);
                Assert.AreEqual(1, inters.Count);
                Assert.AreEqual(30, inters.ElementAt(0).Range);
            }
        }

        [TestMethod]
        public void Align()
        {
            {
                var array = new double[] { 359, 0, 1, 2, 3 };
                Angle.Align(array);
                CollectionAssert.AreEqual(new double[] { 359, 360, 361, 362, 363 }, array);
            }
            {
                var array = new double[] { -3, -7, -11 };
                Angle.Align(array);
                CollectionAssert.AreEqual(new double[] { -3, -7, -11 }, array);
            }
            {
                var array = new double[] { 4, 350, 340 };
                Angle.Align(array);
                CollectionAssert.AreEqual(new double[] { 4, -10, -20 }, array);
            }
            {
                var array = new double[] { 350, 10, 20 };
                Angle.Align(array);
                CollectionAssert.AreEqual(new double[] { 350, 370, 380 }, array);
            }
        }

        [TestMethod]
        public void FromDecimalToDms()
        {
            Assert.AreEqual(new DMS(24, 27, 23.63), new DMS(24.456565));
            Assert.AreEqual(-new DMS(24, 27, 23.63), new DMS(-24.456565));           
            Assert.AreEqual(new DMS(14, 7, 27.57), new DMS(14.12432545435));
        }

        [TestMethod]
        public void FromDmsToDecimal()
        {
            // max error is 1/100 of second of arc
            double error = 1 / 3600.0 / 100;

            Assert.AreEqual(24.456565, new DMS(24, 27, 23.63).ToDecimalAngle(), error);
            Assert.AreEqual(-24.456565, -new DMS(24, 27, 23.63).ToDecimalAngle(), error);
            Assert.AreEqual(14.12432545435, new DMS(14, 7, 27.57).ToDecimalAngle(), error);
        }

        [TestMethod]
        public void FromStringToDms()
        {
            Assert.AreEqual(new DMS(4, 27, 40.386), new DMS("4* 27' 40.386"));
            Assert.AreEqual(new DMS(4, 27, 40.386), new DMS("4*27'40.386''"));
            Assert.AreEqual(-new DMS(4, 27, 40.386), new DMS("-4* 27' 40.386''"));
            Assert.AreEqual(new DMS(4, 27, 40.386), new DMS("+4* 27' 40.386''"));
            Assert.AreEqual(-new DMS(4, 27, 40.386), new DMS("-4 27 40.386"));
        }

        [TestMethod]
        public void FromDecimalToHms()
        {
            Assert.AreEqual(new HMS(4, 27, 40.386), new HMS(66.918277));
            Assert.AreEqual(new HMS(17, 13, 21.13), new HMS(258.33804166));
        }

        [TestMethod]
        public void FromHmsToDecimal()
        {
            // max error is 1/100 of second of arc
            double error = 1 / 3600.0 / 100;

            Assert.AreEqual(66.91825, new HMS(4, 27, 40.38).ToDecimalAngle(), error);
            Assert.AreEqual(258.33804, new HMS(17, 13, 21.13).ToDecimalAngle(), error);
        }

        [TestMethod]
        public void FromStringToHms()
        {
            Assert.AreEqual(new HMS(4, 27, 40.386), new HMS("4h 27m 40.386s"));
            Assert.AreEqual(new HMS(17, 13, 21.13), new HMS("17 13 21.13"));
        }

        [TestMethod]
        public void HmsToString()
        {
            Func<HMS, string> formatter = (HMS hms) => string.Format(CultureInfo.InvariantCulture, "{0:D2}ч {1:D2}м {2:.##}с", hms.Hours, hms.Minutes, hms.Seconds);
            Assert.AreEqual("04ч 27м 40.39с", new HMS(4, 27, 40.386).ToString(formatter));
        }

        [TestMethod]
        public void DmsToString()
        {
            Func<DMS, string> formatter = (DMS dms) => string.Format(CultureInfo.InvariantCulture, "{0:#}° {1:D2}' {2:0.##}''", dms.Degrees, dms.Minutes, dms.Seconds);
            Assert.AreEqual("4° 27' 40.39''", new DMS(4, 27, 40.386).ToString(formatter));
        }

        [TestMethod]
        public void Separation()
        {
            // Example 17.a, AA(II)
            {
                var c1 = new CrdsEquatorial(213.9154, 19.1825);
                var c2 = new CrdsEquatorial(201.2983, -11.1614);
                Assert.AreEqual(32.7930, Angle.Separation(c1, c2), 1e-4);
            }
            {
                var c1 = new CrdsEcliptical(213.9154, 19.1825);
                var c2 = new CrdsEcliptical(201.2983, -11.1614);
                Assert.AreEqual(32.7930, Angle.Separation(c1, c2), 1e-4);
            }
            {
                var c1 = new CrdsGeographical(213.9154, 19.1825);
                var c2 = new CrdsGeographical(201.2983, -11.1614);
                Assert.AreEqual(32.7930, Angle.Separation(c1, c2), 1e-4);
            }
            {
                var c1 = new CrdsHorizontal(213.9154, 19.1825);
                var c2 = new CrdsHorizontal(201.2983, -11.1614);
                Assert.AreEqual(32.7930, Angle.Separation(c1, c2), 1e-4);
            }
        }

        [TestMethod]
        public void Intermediate()
        {
            CrdsHorizontal h1 = new CrdsHorizontal(5, 52);
            CrdsHorizontal h2 = new CrdsHorizontal(-120, 37);

            var expected = new CrdsHorizontal[]
            {
                new CrdsHorizontal(5, 52),
                new CrdsHorizontal(-9.924971, 59.60085),
                new CrdsHorizontal(-31.666402, 64.65469),
                new CrdsHorizontal(-58.557896, 65.51770),
                new CrdsHorizontal(-82.746574, 61.80095),
                new CrdsHorizontal(-99.938739, 54.93700),
                new CrdsHorizontal(-111.623334, 46.39746),
                new CrdsHorizontal(-120.000000, 37.00000)
            };

            for (int i = 0; i <= 7; i++)
            {
                var h = Angle.Intermediate(h1, h2, i / 7.0);

                var altExpected = expected[i].Altitude;
                var azExpected = expected[i].Azimuth;

                Assert.AreEqual(altExpected, h.Altitude, 1e-5);
                Assert.AreEqual(azExpected, h.Azimuth, 1e-5);
            }
        }
    }
}
