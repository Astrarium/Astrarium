using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ADK
{
    /// <summary>
    /// Contains methods for calculating ephemeris of planets
    /// </summary>
    public static class PlanetPositions
    {
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
        private static List<Term>[,,] TermsHP = null;

        /// <summary>
        /// VSOP87 terms to calculate Low-Precision planet positions
        /// </summary>
        /// <remarks>
        /// These values are taken from AA2 book, Appendix III.
        /// </remarks>
        private static List<Term>[,,] TermsLP = null;
          
        /// <summary>
        /// Loads VSOP terms from file if not loaded yet
        /// </summary>
        private static void Initialize(bool highPrecision)
        {
            // high precision
            if (highPrecision && TermsHP == null)
            {
                TermsHP = new List<Term>[PLANETS_COUNT, SERIES_COUNT, POWERS_COUNT];
            }
            // low precision
            else if (!highPrecision && TermsLP == null)
            {
                TermsLP = new List<Term>[PLANETS_COUNT, SERIES_COUNT, POWERS_COUNT];
            }
            // already initialized
            else
            {
                return;
            }

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"ADK.Data.VSOP87.{(highPrecision ? "HP" : "LP")}.bin"))
            using (var reader = new BinaryReader(stream))
            {
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    int p = reader.ReadInt32();
                    int t = reader.ReadInt32();
                    int power = reader.ReadInt32();
                    int count = reader.ReadInt32();

                    double A, B, C;

                    if (highPrecision)
                    {
                        TermsHP[p, t, power] = new List<Term>();
                    }
                    else
                    {
                        TermsLP[p, t, power] = new List<Term>();
                    }

                    for (int i = 0; i < count; i++)
                    {
                        A = reader.ReadDouble();
                        B = reader.ReadDouble();
                        C = reader.ReadDouble();

                        if (highPrecision)
                        {
                            TermsHP[p, t, power].Add(new Term(A, B, C));
                        }
                        else
                        {
                            TermsLP[p, t, power].Add(new Term(A, B, C));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculates heliocentrical coordinates of the planet using VSOP87 motion theory. 
        /// </summary>
        /// <param name="planet"><see cref="Planet"/> to calculate heliocentrical coordinates.</param>
        /// <param name="jde">Julian Ephemeris Day</param>
        /// <returns>Returns heliocentric coordinates of the planet for given date.</returns>
        public static CrdsHeliocentrical GetPlanetCoordinates(Planet planet, double jde, bool highPrecision = true)
        {
            Initialize(highPrecision);

            const int L = 0;
            const int B = 1;
            const int R = 2;

            int p = (int)planet - 1;

            double t = (jde - 2451545.0) / 365250.0;

            double t2 = t * t;
            double t3 = t * t2;
            double t4 = t * t3;
            double t5 = t * t4;

            double[] l = new double[6];
            double[] b = new double[6];
            double[] r = new double[6];

            List<Term>[,,] terms = highPrecision ? TermsHP : TermsLP;

            for (int j = 0; j < 6; j++)
            {
                l[j] = 0;
                for (int i = 0; i < terms[p, L, j]?.Count; i++)
                {
                    l[j] += terms[p, L, j][i].A * Math.Cos(terms[p, L, j][i].B + terms[p, L, j][i].C * t);
                }
                b[j] = 0;
                for (int i = 0; i < terms[p, B, j]?.Count; i++)
                {
                    b[j] += terms[p, B, j][i].A * Math.Cos(terms[p, B, j][i].B + terms[p, B, j][i].C * t);
                }
                r[j] = 0;
                for (int i = 0; i < terms[p, R, j]?.Count; i++)
                {
                    r[j] += terms[p, R, j][i].A * Math.Cos(terms[p, R, j][i].B + terms[p, R, j][i].C * t);
                }
            }

            // Dimension coefficient.
            // Should be applied for the shortened VSOP87 formulae listed in AA book
            // because "A" coefficient expressed in 1e-8 radian for longitude and latitude and in 1e-8 AU for radius vector
            // Original (high-precision) version of VSOP has "A" expressed in radians and AU respectively.
            double d = highPrecision ? 1 : 1e-8;

            CrdsHeliocentrical result = new CrdsHeliocentrical();

            result.L = (l[0] + l[1] * t + l[2] * t2 + l[3] * t3 + l[4] * t4 + l[5] * t5) * d;
            result.L = Angle.ToDegrees(result.L);
            result.L = Angle.To360(result.L);

            result.B = (b[0] + b[1] * t + b[2] * t2 + b[3] * t3 + b[4] * t4 + b[5] * t5) * d;
            result.B = Angle.ToDegrees(result.B);
                
            result.R = (r[0] + r[1] * t + r[2] * t2 + r[3] * t3 + r[4] * t4 + r[5] * t5) * d;

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
