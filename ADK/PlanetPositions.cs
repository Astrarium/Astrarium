using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ADK
{
    /// <summary>
    /// Contains methods for calculating ephemeris of planets
    /// </summary>
    public static class PlanetPositions
    {
        /// <summary>
        /// Epochs count
        /// </summary>
        private const int EPOCHS_COUNT = 2;

        /// <summary>
        /// Total number of planets
        /// </summary>
        private const int PLANETS_COUNT = 8;

        /// <summary>
        /// Number of series, namely L,B,R (3 total)
        /// </summary>
        private const int SERIES_COUNT  = 3;

        /// <summary>
        /// Maximal number of coefficients before powers of tau (for example, L0, L2, ..., L5)
        /// </summary>
        private const int POWERS_COUNT  = 6;

        /// <summary>
        /// VSOP87 terms to calculate High-Precision planet positions
        /// </summary>
        /// <remarks>
        /// These values are obtained from original VSOP87 theory,
        /// namely VSOP87D series are used in the implementation.
        /// See <see href="ftp://ftp.imcce.fr/pub/ephem/planets/vsop87/" />
        /// </remarks>
        private static List<Term>[,,,] Terms = null;

        /// <summary>
        /// Number of terms for calculating low-precision planet positions
        /// </summary>
        private static int[,,,] LPTermsCount = null;
          
        /// <summary>
        /// Loads VSOP terms from file if not loaded yet
        /// </summary>
        private static void Initialize()
        {
            // high precision
            if (Terms == null)
            {
                Terms = new List<Term>[EPOCHS_COUNT, PLANETS_COUNT, SERIES_COUNT, POWERS_COUNT];
                LPTermsCount = new int[EPOCHS_COUNT, PLANETS_COUNT, SERIES_COUNT, POWERS_COUNT];
            }
            // already initialized
            else
            {
                return;
            }

            for (int e = 0; e < 2; e++)
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"ADK.Data.VSOP87{(e == 0 ? "B" : "D")}.bin"))
                using (var reader = new BinaryReader(stream))
                {
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        int p = reader.ReadInt32();
                        int t = reader.ReadInt32();
                        int power = reader.ReadInt32();
                        int count = reader.ReadInt32();

                        double A, B, C;

                        Terms[e, p, t, power] = new List<Term>();

                        for (int i = 0; i < count; i++)
                        {
                            A = reader.ReadDouble();
                            B = reader.ReadDouble();
                            C = reader.ReadDouble();
                            Terms[e, p, t, power].Add(new Term(A, B, C));
                        }

                        // sort terms by A coefficient descending
                        Terms[e, p, t, power].Sort((x, y) => Math.Sign(y.A - x.A));
                        LPTermsCount[e, p, t, power] = count;

                        for (int i = 0; i < count; i++)
                        {
                            // L, B
                            if (t < 2)
                            {
                                // maximal error in seconds of arc
                                // calculated by rule descibed in AA(II), p. 220 (Accuracy of the results)
                                double err = Angle.ToDegrees(2 * Math.Sqrt(i + 1) * Terms[e, p, t, power][i].A) * 3600;
                                if (err < 1)
                                {
                                    LPTermsCount[e, p, t, power] = i;
                                    break;
                                }
                            }
                            // R
                            else
                            {
                                // maximal error in AU
                                // calculated by rule descibed in AA(II), p. 220 (Accuracy of the results)
                                double err = 2 * Math.Sqrt(i + 1) * Terms[e, p, t, power][i].A;
                                if (err < 1e-8)
                                {
                                    LPTermsCount[e, p, t, power] = i;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculates heliocentrical coordinates of the planet using VSOP87 motion theory. 
        /// </summary>
        /// <param name="planet">Planet serial number - from 1 (Mercury) to 8 (Neptune) to calculate heliocentrical coordinates.</param>
        /// <param name="jde">Julian Ephemeris Day</param>
        /// <returns>Returns heliocentric coordinates of the planet for given date.</returns>
        public static CrdsHeliocentrical GetPlanetCoordinates(int planet, double jde, bool highPrecision = true, bool epochOfDate = true)
        {
            Initialize();

            const int L = 0;
            const int B = 1;
            const int R = 2;

            int p = planet - 1;
            int e = epochOfDate ? 1 : 0;

            double t = (jde - 2451545.0) / 365250.0;

            double t2 = t * t;
            double t3 = t * t2;
            double t4 = t * t3;
            double t5 = t * t4;

            double[] l = new double[6];
            double[] b = new double[6];
            double[] r = new double[6];

            for (int j = 0; j < 6; j++)
            {
                l[j] = 0;
                for (int i = 0; i < Terms[e, p, L, j]?.Count; i++)
                {
                    if (!highPrecision && i > LPTermsCount[e, p, L, j])
                    {
                        break;
                    }

                    l[j] += Terms[e, p, L, j][i].A * Math.Cos(Terms[e, p, L, j][i].B + Terms[e, p, L, j][i].C * t);
                }
                b[j] = 0;
                for (int i = 0; i < Terms[e, p, B, j]?.Count; i++)
                {
                    if (!highPrecision && i > LPTermsCount[e, p, B, j])
                    {
                        break;
                    }

                    b[j] += Terms[e, p, B, j][i].A * Math.Cos(Terms[e, p, B, j][i].B + Terms[e, p, B, j][i].C * t);
                }
                r[j] = 0;
                for (int i = 0; i < Terms[e, p, R, j]?.Count; i++)
                {
                    if (!highPrecision && i > LPTermsCount[e, p, R, j])
                    {
                        break;
                    }

                    r[j] += Terms[e, p, R, j][i].A * Math.Cos(Terms[e, p, R, j][i].B + Terms[e, p, R, j][i].C * t);
                }
            }

            CrdsHeliocentrical result = new CrdsHeliocentrical();

            result.L = l[0] + l[1] * t + l[2] * t2 + l[3] * t3 + l[4] * t4 + l[5] * t5;
            result.L = Angle.ToDegrees(result.L);
            result.L = Angle.To360(result.L);

            result.B = b[0] + b[1] * t + b[2] * t2 + b[3] * t3 + b[4] * t4 + b[5] * t5;
            result.B = Angle.ToDegrees(result.B);
                
            result.R = r[0] + r[1] * t + r[2] * t2 + r[3] * t3 + r[4] * t4 + r[5] * t5;

            return result;
        }

        /// <summary>
        /// Gets correction for ecliptical coordinates obtained by VSOP87 theory, 
        /// needed for conversion to standard FK5 system.
        /// This correction should be used only for high-precision version of VSOP87.
        /// </summary>
        /// <param name="jde">Julian Ephemeris Day</param>
        /// <param name="ecl">Non-corrected ecliptical coordinates of the body.</param>
        /// <returns>Corrections values for longutude and latitude, in degrees.</returns>
        /// <remarks>
        /// AA(II), formula 32.3.
        /// </remarks>
        public static CrdsEcliptical CorrectionForFK5(double jde, CrdsEcliptical ecl)
        {
            double T = (jde - 2451545) / 36525.0;

            double L_ = Angle.ToRadians(ecl.Lambda - 1.397 * T - 0.00031 * T * T);

            double sinL_ = Math.Sin(L_);
            double cosL_ = Math.Cos(L_);

            double deltaL = -0.09033 + 0.03916 * (cosL_ + sinL_) * Math.Tan(Angle.ToRadians(ecl.Beta));
            double deltaB = 0.03916 * (cosL_ - sinL_);

            return new CrdsEcliptical(deltaL / 3600, deltaB / 3600);
        }

        /// <summary>
        /// Calculates time taken by the light to reach the Earth from a celestial body.
        /// </summary>
        /// <param name="distance">Distance to celestial body, in A.U.</param>
        /// <returns>Time, in days, taken by the light to reach the Earth.</returns>
        /// <remarks>
        /// This method is a formula 33.3 from AA(II) (page 224).
        /// </remarks>
        public static double LightTimeEffect(double distance)
        {
            return 0.0057755183 * distance;
        }

        /// <summary>
        /// Represents triplet of VSOP87 terms used for calculation of heliocentrical planet positions
        /// </summary>
        private class Term
        {
            /// <summary>
            /// A coefficient
            /// </summary>
            public double A { get; private set; }

            /// <summary>
            /// B coefficient
            /// </summary>
            public double B { get; private set; }

            /// <summary>
            /// C coefficient
            /// </summary>
            public double C { get; private set; }

            /// <summary>
            /// Creates new VSOP87 term.
            /// </summary>
            /// <param name="a">A coefficient</param>
            /// <param name="b">B coefficient</param>
            /// <param name="c">C coefficient</param>
            public Term(double a, double b, double c)
            {
                A = a;
                B = b;
                C = c;
            }
        }
    }
}
