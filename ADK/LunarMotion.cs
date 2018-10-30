using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace ADK
{
    public static class LunarMotion
    {
        private static bool IsInitialized = false;

        private static sbyte[,] ARGS_B = new sbyte[60, 4];
        private static sbyte[,] ARGS_LR = new sbyte[60, 4];
        private static int[] B_C = new int[60];
        private static int[] COS_C = new int[60];
        private static int[] SIN_C = new int[60];

        public static CrdsEcliptical GetCoordinates(double jd)
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
                            ARGS_LR[i, j] = reader.ReadSByte();
                        }
                        SIN_C[i] = reader.ReadInt32();
                        COS_C[i] = reader.ReadInt32();
                        for (int j = 0; j < 4; j++)
                        {
                            ARGS_B[i, j] = reader.ReadSByte();
                        }
                        B_C[i] = reader.ReadInt32();
                    }
                }
            }

            double T = (jd - 2451545.0) / 36525.0;

            double T2 = T * T;
            double T3 = T2 * T;
            double T4 = T3 * T;

            double L_ = 218.3164477 + 481267.88123421 * T - 0.0015786 * T2 + T3 / 538841.0 - T4 / 65194000.0;
            double D = 297.8501921 + 445267.1114034 * T - 0.0018819 * T2 + T3 / 545868.0 - T4 / 113065000.0;
            double M = 357.5291092 + 35999.0502909 * T - 0.0001536 * T2 + T3 / 24490000.0;
            double M_ = 134.9633964 + 477198.8675055 * T + 0.0087414 * T2 + T3 / 69699.0 - T4 / 14712000.0;
            double F = 93.2720950 + 483202.0175233 * T - 0.0036539 * T2 - T3 / 3526000.0 + T4 / 863310000.0;
            double A1 = 119.75 + 131.849 * T;
            double A2 = 53.09 + 479264.290 * T;
            double A3 = 313.45 + 481266.484 * T;
            double E = 1 - 0.002516 * T - 0.0000074 * T2;

            L_ = Angle.To360(L_);
            D = Angle.To360(D);
            M = Angle.To360(M);
            M_ = Angle.To360(M_);
            F = Angle.To360(F);
            A1 = Angle.To360(A1);
            A2 = Angle.To360(A2);
            A3 = Angle.To360(A3);

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
                    lrArg += DMMF[j] * ARGS_LR[i, j];
                    bArg += DMMF[j] * ARGS_B[i, j];
                }

                Sum_l += SIN_C[i] * Math.Sin(lrArg) * powE[Math.Abs(ARGS_LR[i, 1])];
                Sum_r += COS_C[i] * Math.Cos(lrArg) * powE[Math.Abs(ARGS_LR[i, 1])];
                Sum_b += B_C[i] * Math.Sin(bArg) * powE[Math.Abs(ARGS_B[i, 1])];
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

            double L_deg = Angle.ToDegrees(L_);

            ecl.Lambda = L_deg + Sum_l / 1000000.0;
            ecl.Lambda = Angle.To360(ecl.Lambda);
            ecl.Beta = Sum_b / 1000000.0;
            ecl.Distance = 385000.56 + Sum_r / 1000.0;

            return ecl;
        }
    }
}
