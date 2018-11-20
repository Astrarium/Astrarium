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
        public void CalculatePlanetApparentPlace()
        {
            // TODO: test not pass

            double jde = 2448976.5;

            CrdsHeliocentrical hEarth = PlanetPositions.GetPlanetCoordinates(3, jde, highPrecision: false);
            Assert.AreEqual(88.35704, hEarth.L, 1e-5);
            Assert.AreEqual(0.00014, hEarth.B, 1e-5);
            Assert.AreEqual(0.983824, hEarth.R, 1e-6);

            CrdsHeliocentrical hVenus = PlanetPositions.GetPlanetCoordinates(2, jde, highPrecision: false);
            Assert.AreEqual(26.11428, hVenus.L, 1e-5);
            Assert.AreEqual(-2.62070, hVenus.B, 1e-5);
            Assert.AreEqual(0.724603, hVenus.R, 1e-6);

            CrdsRectangular rect = hVenus.ToRectangular(hEarth);
            Assert.AreEqual(0.621746, rect.X, 1e-6);
            Assert.AreEqual(-0.664810, rect.Y, 1e-6);
            Assert.AreEqual(-0.033134, rect.Z, 1e-6);

            double delta = rect.ToEcliptical().Distance;
            Assert.AreEqual(0.910845, delta, 1e-6);

            double tau = 0.0057755183 * rect.ToEcliptical().Distance;
            Assert.AreEqual(0.0052606, tau, 1e-7);

            hVenus = PlanetPositions.GetPlanetCoordinates(2, jde - tau, highPrecision: false);
            Assert.AreEqual(26.10588, hVenus.L, 1e-5);
            Assert.AreEqual(-2.62102, hVenus.B, 1e-5);
            Assert.AreEqual(0.724604, hVenus.R, 1e-6);

            rect = hVenus.ToRectangular(hEarth);
            Assert.AreEqual(0.621794, rect.X, 1e-6);
            Assert.AreEqual(-0.664905, rect.Y, 1e-6);
            Assert.AreEqual(-0.033138, rect.Z, 1e-6);

            // Ecliptical coordinates of Venus.
            // Corrected for light time, but not yet for aberration.
            CrdsEcliptical ecl = rect.ToEcliptical();
            Assert.AreEqual(313.08097, ecl.Lambda, 1e-5);
            Assert.AreEqual(-2.08474, ecl.Beta, 1e-5);

            AberrationElements ae = Aberration.AberrationElements(jde);
            ae.lambda = Angle.To360(hEarth.L + 180);

            Assert.AreEqual(0.016711589, ae.e, 1e-9);
            Assert.AreEqual(102.81644, ae.pi, 1e-5);
            Assert.AreEqual(268.35704, ae.lambda, 1e-5);

            CrdsEcliptical deltaEcl = Aberration.AberrationEffect(ecl, ae);
            Assert.AreEqual(-14.868, deltaEcl.Lambda * 3600, 1e-3);
            Assert.AreEqual(-0.531, deltaEcl.Beta * 3600, 1e-3);

            ecl += deltaEcl;
            Assert.AreEqual(313.07684, ecl.Lambda, 1e-5);
            Assert.AreEqual(-2.08489, ecl.Beta, 1e-5);

            CrdsEcliptical fk5corr = PlanetPositions.CorrectionForFK5(jde, ecl);
            Assert.AreEqual(-0.09027, fk5corr.Lambda * 3600, 1e-5);
            Assert.AreEqual(0.05535, fk5corr.Beta * 3600, 1e-5);
            ecl += fk5corr;

            Assert.AreEqual(313.07686, ecl.Lambda, 1e-5);
            Assert.AreEqual(-2.08487, ecl.Beta, 1e-5);

            ecl += Nutation.NutationEffect(16.749 / 3600.0);

            CrdsEquatorial eq = ecl.ToEquatorial(23.439669);

            Assert.AreEqual(new HMS("21h 04m 41.50s"), new HMS(eq.Alpha));
            Assert.AreEqual(new DMS("-18* 53' 16.84''"), new DMS(eq.Delta));
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
