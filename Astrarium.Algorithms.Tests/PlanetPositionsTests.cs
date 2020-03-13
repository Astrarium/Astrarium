using Astrarium.Algorithms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Astrarium.Algorithms.Tests
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

            var lines = ReadLinesFromResource("Astrarium.Algorithms.Tests.Data.VSOP87.chk", Encoding.ASCII).ToList();

            Regex regexHeader = new Regex(@"^VSOP87[BD]\s+([\w]+)\s*JD([\w\d\.]+).*$");
            Regex regexValues = new Regex(@"^\s*l\s+([-\d.]+)\s+rad\s+b\s+([-\d.]+)\s+rad\s+r\s+([\d.]+)\s+au\s*$");

            for (int i = 0; i < lines.Count; i++)
            {
                string header = lines[i].Trim();

                if (header.StartsWith("VSOP87B") || header.StartsWith("VSOP87D"))
                {
                    bool isEpochOfDate = header.StartsWith("VSOP87D");

                    string[] chunks = regexHeader.Match(header).Groups.Cast<Group>().Select(g => g.Value).ToArray();

                    int planet = GetPlanet(FirstCharToUpper(chunks[1].ToLower()));
                    double jd = Double.Parse(chunks[2], numericFormat);

                    i++;
                    string[] values = regexValues.Match(lines[i]).Groups.Cast<Group>().Select(g => g.Value).ToArray();

                    double L = Angle.ToDegrees(Double.Parse(values[1], numericFormat));
                    double B = Angle.ToDegrees(Double.Parse(values[2], numericFormat));
                    double R = double.Parse(values[3], numericFormat);

                    testData.Add(new VSOP87DTestData(isEpochOfDate, planet, jd, L, B, R));
                }
            }
        }

        [TestMethod]
        public void GetPlanetCoordinatesLP()
        {
            // Test low-precision implementation from AA2 book
            foreach (VSOP87DTestData testValue in testData)
            {
                CrdsHeliocentrical crds = PlanetPositions.GetPlanetCoordinates(testValue.Planet, testValue.JDE, highPrecision: false, epochOfDate: testValue.IsEpochOfDate);

                double deltaL = Math.Abs(testValue.L - crds.L) * SECONDS_IN_DEGREE;
                double deltaB = Math.Abs(testValue.B - crds.B) * SECONDS_IN_DEGREE;
                double deltaR = Math.Abs(testValue.R - crds.R);

                // difference in L should be less than 2" of arc
                Assert.IsTrue(deltaL < 2);

                // difference in B should be less than 2" of arc
                Assert.IsTrue(deltaB < 2);

                // difference in R should be less than 1e-5 AU
                Assert.IsTrue(deltaR < 1e-5);
            }
        }

        [TestMethod]
        public void GetPlanetCoordinatesHP()
        {
            // Test high-precision implementation from original VSOP87 theory.
            foreach (VSOP87DTestData testValue in testData)
            {
                CrdsHeliocentrical crds = PlanetPositions.GetPlanetCoordinates(testValue.Planet, testValue.JDE, highPrecision: true, epochOfDate: testValue.IsEpochOfDate);

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

            // transform to ecliptical coordinates of the Sun
            CrdsEcliptical ecl = new CrdsEcliptical(Angle.To360(crds.L + 180), -crds.B, crds.R);

            // get FK5 system correction
            CrdsEcliptical corr = PlanetPositions.CorrectionForFK5(jde, ecl);

            // correct solar coordinates to FK5 system
            ecl += corr;

            var nutation = Nutation.NutationElements(jde);

            // True obliquity
            double epsilon = Date.TrueObliquity(jde, nutation.deltaEpsilon);

            // add nutation effect
            ecl += Nutation.NutationEffect(nutation.deltaPsi);

            // calculate aberration effect 
            CrdsEcliptical aberration = Aberration.AberrationEffect(ecl.Distance);

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
            double jde = 2448976.5;

            double tau = 0;
            CrdsEcliptical ecl = null;

            for (int i = 0; i < 2; i++)
            {
                CrdsHeliocentrical hEarth = PlanetPositions.GetPlanetCoordinates(3, jde - tau, highPrecision: false);

                CrdsHeliocentrical hVenus = PlanetPositions.GetPlanetCoordinates(2, jde - tau, highPrecision: false);

                var rect = hVenus.ToRectangular(hEarth);

                ecl = rect.ToEcliptical();
               
                tau = PlanetPositions.LightTimeEffect(ecl.Distance);
            }

            // Correction for FK5 system
            CrdsEcliptical corr = PlanetPositions.CorrectionForFK5(jde, ecl);            
            ecl += corr;
            ecl += Nutation.NutationEffect(16.749 / 3600.0);

            CrdsEquatorial eq = ecl.ToEquatorial(23.439669);

            Assert.AreEqual(new HMS("21h 04m 41.459s"), new HMS(eq.Alpha));
            Assert.AreEqual(new DMS("-18* 53' 16.66''"), new DMS(eq.Delta));
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
            /// Flag indicating that test data is for epoch of date
            /// </summary>
            public bool IsEpochOfDate { get; private set; }

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
            /// <param name="isEpochOfDate">Test data is for epoch of date</param>
            /// <param name="planet">Planet</param>
            /// <param name="jde">Julian Ephemeris Day</param>
            /// <param name="L">L value</param>
            /// <param name="B">B value</param>
            /// <param name="R">R value</param>
            public VSOP87DTestData(bool isEpochOfDate, int planet, double jde, double L, double B, double R)
            {
                IsEpochOfDate = isEpochOfDate;
                Planet = planet;
                JDE = jde;
                this.L = L;
                this.B = B;
                this.R = R;
            }
        }
    }
}
