using System;
using static System.Math;
using static ADK.Angle;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK
{
    /// <summary>
    /// Provides methods for calculation of positions of minor planets and comets.
    /// </summary>
    public static class MinorBodyPositions
    {
        /// <summary>
        /// Performs reduction of orbital elements from one epoch (oe0.Epoch) to another (jd).
        /// </summary>
        /// <param name="oe"></param>
        /// <param name="jd"></param>
        /// <returns></returns>
        /// <remarks>Method is taken from AA(II), ch. 24.</remarks>
        public static OrbitalElements Reduction(OrbitalElements oe0, double jd0, double jd)
        {
            OrbitalElements oe = new OrbitalElements(oe0);
            //oe.Epoch = jd;

            double T = (jd0 - 2451545.0) / 36525.0;
            double t = (jd - jd0) / 36525.0;

            double T2 = T * T;
            double t2 = t * t;
            double t3 = t2 * t;

            // Formulae 21.5
            double eta = (47.0029 - 0.06603 * T + 0.000598 * T2) * t
                + (-0.03302 + 0.000598 * T) * t2 + 0.000060 * t3;

            eta /= 3600;

            double PI = 3289.4789 * T + 0.60622 * T2 -
                (869.8089 + 0.50491 * T) * t + 0.03536 * t2;

            PI = 174.876384 + PI / 3600;
            PI = To360(PI);

            double p = (5029.0966 + 2.22226 * T - 0.000042 * T2) * t
                + (1.11113 - 0.000042 * T) * t2 - 0.000006 * t3;

            p /= 3600;

            double psi = PI + p;

            // Formulae 24.2
            double A = Sin(ToRadians(oe0.i)) * Sin(ToRadians(oe0.Omega - PI));
            double B = -Sin(ToRadians(eta)) * Cos(ToRadians(oe0.i)) + Cos(ToRadians(eta)) * Sin(ToRadians(oe0.i)) * Cos(ToRadians(oe0.Omega - PI));

            double sinI = Sqrt(A * A + B * B);
            oe.i = ToDegrees(Asin(sinI));

            double OmegaPsi = ToDegrees(Atan2(A, B));
            oe.Omega = To360(OmegaPsi + psi);

            // Formulae 24.3
            double C = -Sin(ToRadians(eta)) * Sin(ToRadians(oe0.Omega - PI));
            double D = Sin(ToRadians(oe0.i)) * Cos(ToRadians(eta)) - Cos(ToRadians(oe0.i)) * Sin(ToRadians(eta)) * Cos(ToRadians(oe0.Omega - PI));

            double deltaOmega = ToDegrees(Atan2(C, D));
            oe.omega = To360(oe.omega + deltaOmega);

            return oe;
        }

        public static CrdsRectangular GetRectangularCoordinates(OrbitalElements oe0, double jd, double epsilon)
        {
            OrbitalElements oe = Reduction(oe0, Date.EPOCH_J2000, jd);

            //var oe = oe0;

            double Omega = ToRadians(oe.Omega);
            epsilon = ToRadians(epsilon);
            double i = ToRadians(oe.i);

            double sinEpsilon = Sin(epsilon);
            double cosEpsilon = Cos(epsilon);
            double sinI = Sin(i);
            double cosI = Cos(i);
            double sinOmega = Sin(Omega);
            double cosOmega = Cos(Omega);

            double F = cosOmega;
            double G = sinOmega * cosEpsilon;
            double H = sinOmega * sinEpsilon;

            double P = -sinOmega * cosI;
            double Q = cosOmega * cosI * cosEpsilon - sinI * sinEpsilon;
            double R = cosOmega * cosI * sinEpsilon + sinI * cosEpsilon;

            double a = Sqrt(F * F + P * P);
            double b = Sqrt(G * G + Q * Q);
            double c = Sqrt(H * H + R * R);

            double A = ToDegrees(Atan2(F, P));
            double B = ToDegrees(Atan2(G, Q));
            double C = ToDegrees(Atan2(H, R));

            OrbitalPosition op = OrbitalPosition(oe, jd);

            return new CrdsRectangular()
            {
                X = op.r * a * Sin(ToRadians(A + oe.omega + op.v)),
                Y = op.r * b * Sin(ToRadians(B + oe.omega + op.v)),
                Z = op.r * c * Sin(ToRadians(C + oe.omega + op.v))
            };
        }

        private static OrbitalPosition OrbitalPosition(OrbitalElements oe, double jd)
        {
            // Elliptic orbit
            if (oe.e < 1)
            {
                return EllipticMotion(oe, jd);
            }
            else if (oe.e > 1)
            {
                return HyperbolicMotion(oe, jd);
            }
            else /* oe0.e == 1 */
            {
                return ParabolicMotion(oe, jd);
            }
        }

        /// <summary>
        /// Calculates orbital elements for elliptic orbit
        /// </summary>
        /// <param name="oe0">Orbital elements for initial epoch</param>
        /// <param name="jd">Target epoch</param>
        private static OrbitalPosition EllipticMotion(OrbitalElements oe, double jd)
        {
            double n = 0.9856076686 / (oe.a * Sqrt(oe.a));
            double nd = jd - oe.Epoch;
            double M = oe.M + n * nd;

            double E = SolveKepler(M, oe.e);

            return new OrbitalPosition()
            {
                v = TrueAnomaly(oe.e, E),
                r = oe.a * (1 - oe.e * Cos(ToRadians(E)))
            }; 
        }

        private static OrbitalPosition HyperbolicMotion(OrbitalElements oe, double jd)
        {
            // Method from AAPlus class framework (http://www.naughter.com/aa.html)

            double k = 0.01720209895;
            double t = jd - oe.Epoch; // time since perihelion 
            double third = 1.0 / 3.0;
            double a = 0.75 * t * k * Sqrt((1 + oe.e) / (oe.q * oe.q * oe.q));
            double b = Sqrt(1 + a * a);
            double W = Pow(b + a, third) - Pow(b - a, third);
            double W2 = W * W;
            double W4 = W2 * W2;
            double f = (1 - oe.e) / (1 + oe.e);
            double a1 = 2.0 / 3 + 0.4 * W2;
            double a2 = 7.0 / 5 + 33.0 / 35 * W2 + 37.0 / 175 * W4;
            double a3 = W2 * (432.0 / 175 + 956.0 / 1125 * W2 + 84.0 / 1575 * W4);
            double C = W2 / (1 + W2);
            double g = f * C * C;
            double w = W * (1 + f * C * (a1 + a2 * g + a3 * g * g));
            double w2 = w * w;

            return new OrbitalPosition()
            {
                v = ToDegrees(2 * Atan(w)),
                r = oe.q * (1 + w2) / (1 + w2 * f)
            };
        }

        private static OrbitalPosition ParabolicMotion(OrbitalElements oe, double jd)
        {
            double t = jd - oe.Epoch; // time since perihelion   
            double W = 0.03649116245 / Pow(oe.q, 1.5) * t;

            double G = W / 2;
            double Y = Pow(G + Sqrt(G * G + 1), 1 / 3.0);
            double s = Y - 1 / Y;

            return new OrbitalPosition()
            {
                v = ToDegrees(2 * Atan(s)),
                r = oe.q * (1 + s * s)
            };
        }

        private static double SolveKepler(double M, double e)
        {
            M = ToRadians(M);
            double E0;
            double E1 = M;
            double M_ = M;
            do
            {
                E0 = E1;
                E1 = M_ + e * Sin(E0);
            } while (Abs(E1 - E0) >= 1e-9);
            return ToDegrees(E1);
        }

        private static double TrueAnomaly(double e, double E)
        {
            double tan_v2 = Sqrt((1 + e) / (1 - e)) * Tan(ToRadians(E / 2));
            return To360(ToDegrees(Atan(tan_v2) * 2));
        }
    }
}
