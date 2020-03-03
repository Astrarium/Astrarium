using System;
using System.IO;
using System.Reflection;

namespace ADK
{
    /// <summary>
    /// Provides methods for calculation of positions of the Moon
    /// </summary>
    public static class LunarMotion
    {
        /// <summary>
        /// Flag indicating the class data is loaded from embedded resources.
        /// </summary>
        private static bool IsInitialized = false;

        /// <summary>
        /// Arguments of periodic terms for latitude (Σb)
        /// </summary>
        private static sbyte[,] ArgsB = new sbyte[60, 4];

        /// <summary>
        /// Arguments of periodic terms for longitude (Σl) and distance (Σr)
        /// </summary>
        private static sbyte[,] ArgsLR = new sbyte[60, 4];

        /// <summary>
        /// Coefficient of the sine of the argument for longitude (Σl) and distance (Σr)
        /// </summary>
        private static int[] SinCoeffLR = new int[60];

        /// <summary>
        /// Coefficient of the cosine of the argument for longitude (Σl) and distance (Σr)
        /// </summary>
        private static int[] CosCoeffLR = new int[60];

        /// <summary>
        /// Coefficient of the sine of the argument for latitude (Σb)
        /// </summary>
        private static int[] CoeffB = new int[60];

        /// <summary>
        /// Loads class data from embedded resources, if not loaded yet.
        /// </summary>
        private static void Initialize()
        {
            if (!IsInitialized)
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"ADK.Data.ELP.bin"))
                using (var reader = new BinaryReader(stream))
                {
                    for (int i = 0; i < 60; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            ArgsLR[i, j] = reader.ReadSByte();
                        }
                        SinCoeffLR[i] = reader.ReadInt32();
                        CosCoeffLR[i] = reader.ReadInt32();
                        for (int j = 0; j < 4; j++)
                        {
                            ArgsB[i, j] = reader.ReadSByte();
                        }
                        CoeffB[i] = reader.ReadInt32();
                    }

                    IsInitialized = true;
                }
            }
        }

        /// <summary>
        /// Gets ecliptical coordinates of the Moon for given instant.
        /// </summary>
        /// <param name="jd">Julian Day.</param>
        /// <returns>Geocentric ecliptical coordinates of the Moon, referred to mean equinox of the date.</returns>
        /// <remarks>
        /// This method is taken from AA(II), chapter 47, 
        /// and based on the Charpont ELP-2000/82 lunar theory.
        /// Accuracy of the method is 10" in longitude and 4" in latitude.
        /// </remarks>
        // TODO: use full ELP2000/82 theory:
        // http://totaleclipse.eu/Astronomy/ELP2000.html
        // http://cdsarc.u-strasbg.fr/viz-bin/cat/VI/79
        public static CrdsEcliptical GetCoordinates(double jd)
        {
            Initialize();

            double T = (jd - 2451545.0) / 36525.0;

            double T2 = T * T;
            double T3 = T2 * T;
            double T4 = T3 * T;

            // Moon's mean longitude
            double L_ = 218.3164477 + 481267.88123421 * T - 0.0015786 * T2 + T3 / 538841.0 - T4 / 65194000.0;

            // Preserve the L_ value in degrees
            double Lm = L_;

            // Mean elongation of the Moon
            double D = 297.8501921 + 445267.1114034 * T - 0.0018819 * T2 + T3 / 545868.0 - T4 / 113065000.0;

            // Sun's mean anomaly
            double M = 357.5291092 + 35999.0502909 * T - 0.0001536 * T2 + T3 / 24490000.0;

            // Moon's mean anomaly
            double M_ = 134.9633964 + 477198.8675055 * T + 0.0087414 * T2 + T3 / 69699.0 - T4 / 14712000.0;

            // Moon's argument of latitude (mean dinstance of the Moon from its ascending node)
            double F = 93.2720950 + 483202.0175233 * T - 0.0036539 * T2 - T3 / 3526000.0 + T4 / 863310000.0;

            // Correction arguments
            double A1 = 119.75 + 131.849 * T;
            double A2 = 53.09 + 479264.290 * T;
            double A3 = 313.45 + 481266.484 * T;

            // Multiplier related to the eccentricity of the Earth orbit
            double E = 1 - 0.002516 * T - 0.0000074 * T2;

            L_ = Angle.ToRadians(L_);
            D = Angle.ToRadians(D);
            M = Angle.ToRadians(M);
            M_ = Angle.ToRadians(M_);
            F = Angle.ToRadians(F);
            A1 = Angle.ToRadians(A1);
            A2 = Angle.ToRadians(A2);
            A3 = Angle.ToRadians(A3);

            double Sum_l = 0;
            double Sum_b = 0;
            double Sum_r = 0;

            double[] DMMF = new double[] { D, M, M_, F };
            double[] powE = new double[3] { 1, E, E * E };

            double lrArg, bArg;
            
            for (int i = 0; i < 60; i++)
            {
                lrArg = 0;
                bArg = 0;

                for (int j = 0; j < 4; j++)
                {
                    lrArg += DMMF[j] * ArgsLR[i, j];
                    bArg += DMMF[j] * ArgsB[i, j];
                }

                Sum_l += SinCoeffLR[i] * Math.Sin(lrArg) * powE[Math.Abs(ArgsLR[i, 1])];
                Sum_r += CosCoeffLR[i] * Math.Cos(lrArg) * powE[Math.Abs(ArgsLR[i, 1])];
                Sum_b += CoeffB[i] * Math.Sin(bArg) * powE[Math.Abs(ArgsB[i, 1])];
            }

            Sum_l += 3958 * Math.Sin(A1)
                    + 1962 * Math.Sin(L_ - F)
                    + 318 * Math.Sin(A2);

            Sum_b += -2235 * Math.Sin(L_)
                    + 382 * Math.Sin(A3)
                    + 175 * Math.Sin(A1 - F)
                    + 175 * Math.Sin(A1 + F)
                    + 127 * Math.Sin(L_ - M_)
                    - 115 * Math.Sin(L_ + M_);

            CrdsEcliptical ecl = new CrdsEcliptical();

            ecl.Lambda = Lm + Sum_l / 1e6;
            ecl.Lambda = Angle.To360(ecl.Lambda);
            ecl.Beta = Sum_b / 1e6;
            ecl.Distance = 385000.56 + Sum_r / 1e3;

            return ecl;
        }
    }
}
