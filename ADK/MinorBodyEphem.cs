using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK
{
    /// <summary>
    /// Provides methods for calculation of ephemerides of minor planets and comets.
    /// </summary>
    public static class MinorBodyEphem
    {
        /// <summary>
        /// Performs reduction of orbital elements from one epoch (oe0.Epoch) to another (jd).
        /// </summary>
        /// <param name="oe"></param>
        /// <param name="jd"></param>
        /// <returns></returns>
        /// <remarks>Method is taken from AA(II), ch. 24.</remarks>
        public static OrbitalElements Reduction(OrbitalElements oe0, double jd)
        {
            OrbitalElements oe = new OrbitalElements(oe0);
            oe.Epoch = jd;

            // TODO: find eta, PI, p (see 21.5
            double eta = 0;
            double PI = 0;
            double p = 0;

            double psi = PI + p;

            // TODO: calculate i, omega, OMEGA

            return oe;
        }

        public static CrdsRectangular GetRectangularCoordinates(OrbitalElements oe, double jd, double epsilon)
        {
            double Omega = Angle.ToRadians(oe.Omega);
            epsilon = Angle.ToRadians(epsilon);
            double i = Angle.ToRadians(oe.i);

            double sinEpsilon = Math.Sin(epsilon);
            double cosEpsilon = Math.Cos(epsilon);
            double sinI = Math.Sin(i);
            double cosI = Math.Cos(i);
            double sinOmega = Math.Sin(Omega);
            double cosOmega = Math.Cos(Omega);

            double F = cosOmega;
            double G = sinOmega * cosEpsilon;
            double H = sinOmega * sinEpsilon;

            double P = -sinOmega * cosI;
            double Q = cosOmega * cosI * cosEpsilon - sinI * sinEpsilon;
            double R = cosOmega * cosI * sinEpsilon + sinI * cosEpsilon;

            double a = Math.Sqrt(F * F + P * P);
            double b = Math.Sqrt(G * G + Q * Q);
            double c = Math.Sqrt(H * H + R * R);

            double A = Angle.ToDegrees(Math.Atan2(F, P));
            double B = Angle.ToDegrees(Math.Atan2(G, Q));
            double C = Angle.ToDegrees(Math.Atan2(H, R));

            OrbitalPosition op = OrbitalPosition(oe, jd);

            return new CrdsRectangular()
            {
                X = op.r * a * Math.Sin(Angle.ToRadians(A + oe.omega + op.v)),
                Y = op.r * b * Math.Sin(Angle.ToRadians(B + oe.omega + op.v)),
                Z = op.r * c * Math.Sin(Angle.ToRadians(C + oe.omega + op.v))
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
                // TODO: Hyperbolic orbit
                throw new NotImplementedException();
            }
            else /* oe0.e == 1 */
            {
                // TODO: Parabolic orbit
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Calculates orbital elements for elliptic orbit
        /// </summary>
        /// <param name="oe0">Orbital elements for initial epoch</param>
        /// <param name="jd">Target epoch</param>
        private static OrbitalPosition EllipticMotion(OrbitalElements oe, double jd)
        {
            double n = 0.9856076686 / (oe.a * Math.Sqrt(oe.a));
            double nd = jd - oe.Epoch;
            double M = oe.M + n * nd;

            double E = SolveKepler(M, oe.e);

            return new OrbitalPosition()
            {
                v = TrueAnomaly(oe.e, E),
                r = oe.a * (1 - oe.e * Math.Cos(Angle.ToRadians(E)))
            }; 
        }

        private static double SolveKepler(double M, double e)
        {
            M = Angle.ToRadians(M);
            double E0;
            double E1 = M;
            double M_ = M;
            do
            {
                E0 = E1;
                E1 = M_ + e * Math.Sin(E0);
            } while (Math.Abs(E1 - E0) >= 1e-9);
            return Angle.ToDegrees(E1);
        }

        private static double TrueAnomaly(double e, double E)
        {
            double tan_v2 = Math.Sqrt((1 + e) / (1 - e)) * Math.Tan(Angle.ToRadians(E / 2));
            return Angle.To360(Angle.ToDegrees(Math.Atan(tan_v2) * 2));
        }
    }
}
