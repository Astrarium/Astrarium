using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ADK.Tests
{
    [TestClass]
    public class PlanetPositionsTests : TestClassBase
    {
        /// <summary>
        /// Number of seconds in 1 degree of arc.
        /// </summary>
        private const double SECONDS_IN_DEGREE = 3600;

        /// <summary>
        /// The data to test VSOP87 implementation are taken from <see href="ftp://ftp.imcce.fr/pub/ephem/planets/vsop87/vsop87.chk" />.
        /// </summary>
        private static List<VSOP87DTestData> testData = new List<VSOP87DTestData>();

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            NumberFormatInfo numericFormat = new NumberFormatInfo();
            numericFormat.NumberDecimalSeparator = ".";

            var lines = ReadLinesFromResource("ADK.Tests.Data.VSOP87.chk", Encoding.ASCII).ToList();

            Regex regexHeader = new Regex(@"^VSOP87D\s+([\w]+)\s*JD([\w\d\.]+).*$");
            Regex regexValues = new Regex(@"^\s*l\s+([-\d.]+)\s+rad\s+b\s+([-\d.]+)\s+rad\s+r\s+([\d.]+)\s+au\s*$");

            for (int i = 0; i < lines.Count; i++)
            {
                string header = lines[i].Trim();
                if (header.StartsWith("VSOP87D"))
                {
                    string[] chunks = regexHeader.Match(header).Groups.Cast<Group>().Select(g => g.Value).ToArray();

                    int planet = GetPlanet(FirstCharToUpper(chunks[1].ToLower()));
                    double jd = Double.Parse(chunks[2], numericFormat);

                    i++;
                    string[] values = regexValues.Match(lines[i]).Groups.Cast<Group>().Select(g => g.Value).ToArray();

                    double L = Angle.ToDegrees(Double.Parse(values[1], numericFormat));
                    double B = Angle.ToDegrees(Double.Parse(values[2], numericFormat));
                    double R = Double.Parse(values[3], numericFormat);

                    testData.Add(new VSOP87DTestData(planet, jd, L, B, R));
                }
            }
        }

        [TestMethod]
        public void GetPlanetCoordinatesLP()
        {
            // Example 32.a from AA
            {
                CrdsHeliocentrical crds = PlanetPositions.GetPlanetCoordinates(2, 2448976.5, highPrecision: false);
                Assert.AreEqual(26.11428, crds.L, 1e-5);
                Assert.AreEqual(-2.62070, crds.B, 1e-5);
                Assert.AreEqual(0.724603, crds.R, 1e-6);
            }

            // Test low-precision implementation from AA2 book
            foreach (VSOP87DTestData testValue in testData)
            {
                CrdsHeliocentrical crds = PlanetPositions.GetPlanetCoordinates(testValue.Planet, testValue.JDE, highPrecision: false);

                double deltaL = Math.Abs(testValue.L - crds.L) * SECONDS_IN_DEGREE;
                double deltaB = Math.Abs(testValue.B - crds.B) * SECONDS_IN_DEGREE;
                double deltaR = Math.Abs(testValue.R - crds.R);

                // difference in L should be less than 4" of arc
                Assert.IsTrue(deltaL < 4);

                // difference in B should be less than 4" of arc
                Assert.IsTrue(deltaB < 4);

                // difference in R should be less than 1e-3 AU
                Assert.IsTrue(deltaR < 1e-3);
            }
        }

        [TestMethod]
        public void GetPlanetCoordinatesHP()
        {
            // Test high-precision implementation from original VSOP87 theory.
            foreach (VSOP87DTestData testValue in testData)
            {
                CrdsHeliocentrical crds = PlanetPositions.GetPlanetCoordinates(testValue.Planet, testValue.JDE, highPrecision: true);

                double deltaL = Math.Abs(testValue.L - crds.L) * SECONDS_IN_DEGREE;
                double deltaB = Math.Abs(testValue.B - crds.B) * SECONDS_IN_DEGREE;
                double deltaR = Math.Abs(testValue.R - crds.R);

                // difference in L should be less than 1" of arc
                Assert.IsTrue(deltaL < 1);

                // difference in B should be less than 1" of arc
                Assert.IsTrue(deltaB < 1);

                // difference in R should be less than 1e-5 AU
                Assert.IsTrue(deltaR < 1e-5);
            }
        }

        /// <summary>
        /// AA(II), example 25.b.
        /// </summary>
        [TestMethod]
        public void GetSolarCoordinatesLP()
        {
            double jde = 2448908.5;

            // get Earth coordinates
            CrdsHeliocentrical crds = PlanetPositions.GetPlanetCoordinates(3, jde, highPrecision: false);

            Assert.AreEqual(19.907372, crds.L, 1e-6);
            Assert.AreEqual(-0.644, crds.B * 3600, 1e-3);
            Assert.AreEqual(0.99760775, crds.R, 1e-8);

            // transform to ecliptical coordinates of the Sun
            CrdsEcliptical ecl = new CrdsEcliptical(Angle.To360(crds.L + 180), -crds.B, crds.R);

            // get FK5 system correction
            CrdsEcliptical corr = PlanetPositions.CorrectionForFK5(jde, ecl);
            Assert.AreEqual(-0.09033, corr.Lambda * 3600, 1e-5);
            Assert.AreEqual(-0.023, corr.Beta * 3600, 1e-3);

            // correct solar coordinates to FK5 system
            ecl += corr;

            Assert.AreEqual(199.907347, ecl.Lambda, 1e-6);
            Assert.AreEqual(0.62, ecl.Beta * 3600, 1e-2);
            Assert.AreEqual(0.99760775, ecl.Distance, 1e-8);

            var nutation = Nutation.NutationElements(jde);

            // True obliquity
            double epsilon = Date.TrueObliquity(jde, nutation.deltaEpsilon);

            // accuracy is 0.5"
            Assert.AreEqual(15.908, nutation.deltaPsi * 3600, 0.5);

            // accuracyis 0.1"        
            Assert.AreEqual(-0.308, nutation.deltaEpsilon * 3600, 0.1);

            // accuracy is 0.1"
            Assert.AreEqual(23.4401443, epsilon, 0.1 / 3600.0);

            // add nutation effect
            ecl += Nutation.NutationEffect(nutation.deltaPsi);

            // calculate aberration effect 
            CrdsEcliptical aberration = Aberration.AberrationEffect(ecl.Distance);
            Assert.AreEqual(-20.539, aberration.Lambda * 3600.0, 1e-3);

            // add aberration effect 
            ecl += aberration;

            // convert ecliptical to equatorial coordinates
            CrdsEquatorial eq = ecl.ToEquatorial(epsilon);

            // check apparent equatorial coordinates
            // assume an accuracy of 0.5'' is sufficient
            Assert.AreEqual(198.378178, eq.Alpha, 1.0 / 3600 * 0.5);
            Assert.AreEqual(-7.783871, eq.Delta, 1.0 / 3600 * 0.5);
        }

        /// <summary>
        /// AA(II), example 33.a.
        /// </summary>
        [TestMethod]
        public void CalculatePlanetApparentPlaceLP()
        {
            // TODO: test not pass

            double jde = 2448976.5;

            double tau = 0;
            CrdsEcliptical ecl = null;

            for (int i = 0; i < 2; i++)
            {
                CrdsHeliocentrical hEarth = PlanetPositions.GetPlanetCoordinates(3, jde - tau, highPrecision: false);

                if (i == 0)
                {
                    Assert.AreEqual(88.35704, hEarth.L, 1e-5);
                    Assert.AreEqual(0.00014, hEarth.B, 1e-5);
                    Assert.AreEqual(0.983824, hEarth.R, 1e-6);
                }
                else
                {
                    Assert.AreEqual(88.35168, hEarth.L, 1e-5);
                    Assert.AreEqual(0.00014, hEarth.B, 1e-5);
                    Assert.AreEqual(0.983825, hEarth.R, 1e-6);
                }

                CrdsHeliocentrical hVenus = PlanetPositions.GetPlanetCoordinates(2, jde - tau, highPrecision: false);
                if (i == 0)
                {
                    Assert.AreEqual(26.11428, hVenus.L, 1e-5);
                    Assert.AreEqual(-2.62070, hVenus.B, 1e-5);
                    Assert.AreEqual(0.724603, hVenus.R, 1e-6);
                }
                else
                {
                    Assert.AreEqual(26.10588, hVenus.L, 1e-5);
                    Assert.AreEqual(-2.62102, hVenus.B, 1e-5);
                    Assert.AreEqual(0.724604, hVenus.R, 1e-6);
                }

                var rect = hVenus.ToRectangular(hEarth);
                if (i == 0)
                {
                    Assert.AreEqual(0.621746, rect.X, 1e-6);
                    Assert.AreEqual(-0.664810, rect.Y, 1e-6);
                    Assert.AreEqual(-0.033134, rect.Z, 1e-6);
                }
                else
                {
                    Assert.AreEqual(0.621702, rect.X, 1e-6);
                    Assert.AreEqual(-0.664903, rect.Y, 1e-6);
                    Assert.AreEqual(-0.033138, rect.Z, 1e-6);
                }

                ecl = rect.ToEcliptical();
               
                tau = PlanetPositions.LightTimeEffect(ecl.Distance);

                if (i == 0)
                {
                    Assert.AreEqual(0.910845, ecl.Distance, 1e-6);
                    Assert.AreEqual(0.0052606, tau, 1e-7);
                }
            }

            Assert.AreEqual(313.07684, ecl.Lambda, 1e-5);
            Assert.AreEqual(-2.08489, ecl.Beta, 1e-5);

            // Correction for FK5 system
            CrdsEcliptical corr = PlanetPositions.CorrectionForFK5(jde, ecl);            
            Assert.AreEqual(-0.09027, corr.Lambda * 3600, 1e-5);
            Assert.AreEqual(0.05535, corr.Beta * 3600, 1e-5);

            ecl += corr;
            Assert.AreEqual(313.07682, ecl.Lambda, 1e-5);
            Assert.AreEqual(-2.08488, ecl.Beta, 1e-5);

            ecl += Nutation.NutationEffect(16.749 / 3600.0);

            CrdsEquatorial eq = ecl.ToEquatorial(23.439669);

            Assert.AreEqual(new HMS("21h 04m 41.48s"), new HMS(eq.Alpha));
            Assert.AreEqual(new DMS("-18* 53' 16.91''"), new DMS(eq.Delta));
        }

        /// <summary>
        /// Test values for the full VSOP87 theory 
        /// are taken from AA(II), page 227, end of example 33.a
        /// </summary>
        [TestMethod]
        public void CalculatePlanetApparentPlaceHP()
        {
            double jde = 2448976.5;

            // time taken by the light to reach the Earth
            double tau = 0;

            // previous value of tau to calculate the difference
            double tau0 = 1;

            // final difference to stop iteration process, 1 second of time
            double deltaTau = TimeSpan.FromSeconds(1).TotalDays;

            // Ecliptical coordinates of Venus
            CrdsEcliptical ecl = null;

            // Iterative process to find ecliptical coordinates of Venus
            while (Math.Abs(tau - tau0) > deltaTau)
            {
                // Heliocentrical coordinates of Earth
                var hEarth = PlanetPositions.GetPlanetCoordinates(3, jde - tau, highPrecision: true);

                // Heliocentrical coordinates of Venus
                var hVenus = PlanetPositions.GetPlanetCoordinates(2, jde - tau, highPrecision: true);

                // Ecliptical coordinates of Venus
                ecl = hVenus.ToRectangular(hEarth).ToEcliptical();

                tau0 = tau;
                tau = PlanetPositions.LightTimeEffect(ecl.Distance);
            }

            // Correction for FK5 system
            ecl += PlanetPositions.CorrectionForFK5(jde, ecl);

            // Take nutation into account
            ecl += Nutation.NutationEffect(16.749 / 3600.0);

            // Apparent equatorial coordinates of Venus
            CrdsEquatorial eq = ecl.ToEquatorial(23.439669);

            Assert.AreEqual(new HMS("21h 04m 41.454s"), new HMS(eq.Alpha));
            Assert.AreEqual(new DMS("-18* 53' 16.82''"), new DMS(eq.Delta));
        }

        /// <summary>
        /// Converts first char of string to uppercase
        /// </summary>
        /// <param name="input">Input string</param>
        /// <returns>Returns modified string with first uppercase character</returns>
        private static string FirstCharToUpper(string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input.First().ToString().ToUpper() + input.Substring(1);
            }
        }

        /// <summary>
        /// Planets names
        /// </summary>
        private static string[] PlanetNames = new string[] {
            "Mercury",
            "Venus",
            "Earth",
            "Mars",
            "Jupiter",
            "Saturn",
            "Uranus",
            "Neptune"
        };

        /// <summary>
        /// Gets planet ordering number from its string name
        /// </summary>
        /// <param name="value">String planet name</param>
        /// <returns>Ordering number of a planet</returns>
        private static int GetPlanet(string value)
        {
            return Array.IndexOf(PlanetNames, value) + 1;
        }

        /// <summary>
        /// Used to store test data to validate VSOP87 implementation.
        /// </summary>
        private class VSOP87DTestData
        {
            /// <summary>
            /// Planet ordering number (1 = Mercury, 2 = Venus etc.)
            /// </summary>
            public int Planet { get; private set; }

            /// <summary>
            /// Julian Ephemeris Day
            /// </summary>
            public double JDE { get; private set; }

            /// <summary>
            /// Expected heliocentrical longitude
            /// </summary>
            public double L { get; private set; }

            /// <summary>
            /// Expected heliocentrical latitude
            /// </summary>
            public double B { get; private set; }

            /// <summary>
            /// Expected heliocentrical radius vector
            /// </summary>
            public double R { get; private set; }

            /// <summary>
            /// Creates new test data
            /// </summary>
            /// <param name="planet">Planet</param>
            /// <param name="jde">Julian Ephemeris Day</param>
            /// <param name="L">L value</param>
            /// <param name="B">B value</param>
            /// <param name="R">R value</param>
            public VSOP87DTestData(int planet, double jde, double L, double B, double R)
            {
                Planet = planet;
                JDE = jde;
                this.L = L;
                this.B = B;
                this.R = R;
            }
        }
    }
}
