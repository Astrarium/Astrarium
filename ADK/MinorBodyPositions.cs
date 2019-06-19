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
        public static CrdsRectangular GetRectangularCoordinates(OrbitalElements oe, double jd, double epsilon)
        {
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
            double a = oe.a == 0 ? oe.q / (1 - oe.e) : oe.a;
            double n = 0.9856076686 / (a * Sqrt(a));
            double nd = jd - oe.Epoch;
            double M = oe.M + n * nd;

            double E = SolveKepler(M, oe.e);

            return new OrbitalPosition()
            {
                v = TrueAnomaly(oe.e, E),
                r = a * (1 - oe.e * Cos(ToRadians(E)))
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
