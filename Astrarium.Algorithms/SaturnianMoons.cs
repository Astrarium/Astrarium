using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Astrarium.Algorithms.Angle;
using static System.Math;

namespace Astrarium.Algorithms
{
    public static class SaturnianMoons
    {
        /// <summary>
        /// 1 a.u. (astronomical unit) in km
        /// </summary>
        private const double AU = 149597870;

        /// <summary>
        /// Moons radius, in km
        /// </summary>
        private static readonly double[] MR = { 400.0 / 2, 498.0 / 2, 1046.0 / 2, 1120.0 / 2, 1528.0 / 2, 5150.0 / 2, 286.0 / 2, 1460.0 / 2 };

        /// <summary>
        /// Moons absolute magnitudes
        /// </summary>
        private static readonly double[] MH = { 3.3, 2.2, 0.7, 0.8, 0.1, -1.2, 4.8, 1.5 };

        /// <summary>
        /// Saturn equatorial radius, in km
        /// </summary>
        private const double SR = 58232;

        /// <summary>
        /// Calculates visible magnitude of Saturnian moon
        /// </summary>
        /// <param name="r">Distance Earth-Saturn centers, in a.u.</param>
        /// <param name="R">Distance Sun-Saturn centers, in a.u.</param>
        /// <param name="moonIndex">Saturnian moon index, 0-based</param>
        public static float Magnitude(double r, double R, int moonIndex)
        {
            return (float)(MH[moonIndex] + 5 * Log10(r * R));
        }

        /// <summary>
        /// Gets distance from Saturnian moon to Earth, expressed in AU
        /// </summary>
        /// <param name="r">Distance Earth-Saturn centers, in a.u.</param>
        /// <param name="z">Planetocentric z-coordinate of moon, expressed in units of Saturn equatorial radii, negative if moon is closer to the Earth than Saturn</param>
        /// <returns></returns>
        public static double DistanceFromEarth(double r, double z)
        {
            return (r * AU + z * SR) / AU;
        }

        /// <summary>
        /// Gets semidiameter of Saturnian moon in seconds of arc
        /// </summary>
        /// <param name="r">Distance Earth-Saturn centers, in a.u.</param>
        /// <param name="z">Planetocentric z-coordinate of moon, expressed in units of Saturn equatorial radii, negative if moon is closer to the Earth than Saturn</param>
        /// <param name="i">Saturnian moon index, 0-based</param>
        /// <returns></returns>
        public static double MoonSemidiameter(double r, double z, int i)
        {
            return ToDegrees(Atan2(MR[i], r * AU + z * SR)) * 3600;
        }

        public static CrdsRectangular[] Positions(double jd, CrdsHeliocentrical earth, CrdsHeliocentrical saturn)
        {
            // p.324

            var e0 = saturn.ToRectangular(earth).ToEcliptical();

            // Convert coordinates to B1950 epoch = 2433282.4235;
            e0 = ConvertCoordinatesToEquinox(jd, Date.EPOCH_B1950, e0);

            double t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11;

            t1 = jd - 2411093.0;
            t2 = t1 / 365.25;
            t3 = (jd - 2433282.423) / 365.25 + 1950.0;
            t4 = jd - 2411368.0;
            t5 = t4 / 365.25;
            t6 = jd - 2415020.0;
            t7 = t6 / 36525.0;
            t8 = t6 / 365.25;
            t9 = (jd - 2442000.5) / 365.25;
            t10 = jd - 2409786.0;
            t11 = t10 / 36525.0;

            double[] W = new double[9];

            W[0] = 5.095 * (t3 - 1866.39);
            W[1] = 74.4 + 32.39 * t2;
            W[2] = 134.3 + 92.62 * t2;
            W[3] = 32.0 - 0.5118 * t5;
            W[4] = 276.59 + 0.5118 * t5;
            W[5] = 267.2635 + 1222.1136 * t7;
            W[6] = 175.4762 + 1221.5515 * t7;
            W[7] = 2.4891 + 0.002435 * t7;
            W[8] = 113.35 - 0.2597 * t7;

            double s1 = Sin(ToRadians(28.0817));
            double c1 = Cos(ToRadians(28.0817));
            double s2 = Sin(ToRadians(168.8112));
            double c2 = Cos(ToRadians(168.8112));
            double e1 = 0.05589 - 0.000346 * t7;

            double[] lambda = new double[9];
            double[] r = new double[9];
            double[] gamma = new double[9];
            double[] OMEGA = new double[9];

            // MIMAS (I)
            {
                double L = 127.64 + 381.994497 * t1 - 43.57 * Sin(ToRadians(W[0])) - 0.720 * Sin(ToRadians(3 * W[0])) - 0.02144 * Sin(ToRadians(5 * W[0]));
                double p = 106.1 + 365.549 * t2;
                double M = L - p;
                double C = 2.18287 * Sin(ToRadians(M)) + 0.025988 * Sin(ToRadians(2 * M)) + 0.00043 * Sin(ToRadians(3 * M));
                lambda[1] = L + C;
                r[1] = 3.06879 / (1 + 0.01905 * Cos(ToRadians(M + C)));
                gamma[1] = 1.563;
                OMEGA[1] = 54.5 - 365.072 * t2;
            }

            // ENCELADUS (II)
            {
                double L = 200.317 + 262.7319002 * t1 + 0.25667 * Sin(ToRadians(W[1])) + 0.20883 * Sin(ToRadians(W[2]));
                double p = 309.107 + 123.44121 * t2;
                double M = L - p;
                double C = 0.55577 * Sin(ToRadians(M)) + 0.00168 * Sin(ToRadians(2 * M));
                lambda[2] = L + C;
                r[2] = 3.94118 / (1 + 0.00485 * Cos(ToRadians(M + C)));
                gamma[2] = 0.0262;
                OMEGA[2] = 348.0 - 151.95 * t2;
            }

            // TETHYS (III)
            {
                lambda[3] = 285.306 + 190.69791226 * t1 + 2.063 * Sin(ToRadians(W[0])) + 0.03409 * Sin(ToRadians(3 * W[0])) + 0.001015 * Sin(ToRadians(5 * W[0]));
                r[3] = 4.880998;
                gamma[3] = 1.0976;
                OMEGA[3] = 111.33 - 72.2441 * t2;
            }

            // DIONE (IV)
            {
                double L = 254.712 + 131.53493193 * t1 - 0.0215 * Sin(ToRadians(W[1])) - 0.01733 * Sin(ToRadians(W[2]));
                double p = 174.8 + 30.820 * t2;
                double M = L - p;
                double C = 0.24717 * Sin(ToRadians(M)) + 0.00033 * Sin(ToRadians(2 * M));
                lambda[4] = L + C;
                r[4] = 6.24871 / (1 + 0.002157 * Cos(ToRadians(M + C)));
                gamma[4] = 0.0139;
                OMEGA[4] = 232.0 - 30.27 * t2;
            }

            // RHEA (V)
            {
                double p_ = 342.7 + 10.057 * t2;
                double a1 = 0.000265 * Sin(ToRadians(p_)) + 0.001 * Sin(ToRadians(W[4]));
                double a2 = 0.000265 * Cos(ToRadians(p_)) + 0.001 * Cos(ToRadians(W[4]));
                double e = Sqrt(a1 * a1 + a2 * a2);
                double p = ToDegrees(Atan2(a1, a2));
                double N = 345.0 - 10.057 * t2;
                double lambda_ = 359.244 + 79.69004720 * t1 + 0.086754 * Sin(ToRadians(N));
                double i = 28.0362 + 0.346898 * Cos(ToRadians(N)) + 0.01930 * Cos(ToRadians(W[3]));
                double Omega = 168.8034 + 0.736936 * Sin(ToRadians(N)) + 0.041 * Sin(ToRadians(W[3]));
                double a = 8.725924;
                Subroutine(e, lambda_, p, Omega, i, a, out lambda[5], out gamma[5], out OMEGA[5], out r[5]);
            }

            // TITAN (VI)
            {
                double L = 261.1582 + 22.57697855 * t4 + 0.074025 * Sin(ToRadians(W[3]));
                double i_ = 27.45141 + 0.295999 * Cos(ToRadians(W[3]));
                double OMEGA_ = 168.66925 + 0.628808 * Sin(ToRadians(W[3]));
                double a1 = Sin(ToRadians(W[7])) * Sin(ToRadians(OMEGA_ - W[8]));
                double a2 = Cos(ToRadians(W[7])) * Sin(ToRadians(i_)) - Sin(ToRadians(W[7])) * Cos(ToRadians(i_)) * Cos(ToRadians(OMEGA_ - W[8]));
                double g0 = 102.8623;
                double psi = ToDegrees(Atan2(a1, a2));
                double s = Sqrt(a1 * a1 + a2 * a2);
                double g = W[4] - OMEGA_ - psi;
                double w_ = 0;
                for (int j = 0; j < 3; j++)
                {
                    w_ = W[4] + 0.37515 * (Sin((ToRadians(2 * g))) - Sin(ToRadians(2 * g0)));
                    g = w_ - OMEGA_ - psi;
                }
                double e_ = 0.029092 + 0.00019048 * (Cos(ToRadians(2 * g)) - Cos(ToRadians(2 * g0)));
                double q = 2 * (W[5] - w_);
                double b1 = Sin(ToRadians(i_)) * Sin(ToRadians(OMEGA_ - W[8]));
                double b2 = Cos(ToRadians(W[7])) * Sin(ToRadians(i_)) * Cos(ToRadians(OMEGA_ - W[8])) - Sin(ToRadians(W[7])) * Cos(ToRadians(i_));
                double theta = ToDegrees(Atan2(b1, b2)) + W[8];
                double e = e_ + 0.002778797 * e_ * Cos(ToRadians(q));
                double p = w_ + 0.159215 * Sin(ToRadians(q));
                double u = 2 * W[5] - 2 * theta + psi;
                double h = 0.9375 * e_ * e_ * Sin(ToRadians(q)) + 0.1875 * s * s * Sin(2 * ToRadians(W[5] - theta));
                double lambda_ = L - 0.254744 * (e1 * Sin(ToRadians(W[6])) + 0.75 * e1 * e1 * Sin(ToRadians(2 * W[6])) + h);
                double i = i_ + 0.031843 * s * Cos(ToRadians(u));
                double Omega = OMEGA_ + (0.031843 * s * Sin(ToRadians(u))) / Sin(ToRadians(i_));
                double a = 20.216193;
                Subroutine(e, lambda_, p, Omega, i, a, out lambda[6], out gamma[6], out OMEGA[6], out r[6]);
            }

            // HYPERION (VII)
            {
                double eta = 92.39 + 0.5621071 * t6;
                double zeta = 148.19 - 19.18 * t8;
                double theta = 184.8 - 35.41 * t9;
                double theta_ = theta - 7.5;
                double a_s = 176.0 + 12.22 * t8;
                double b_s = 8.0 + 24.44 * t8;
                double c_s = b_s + 5.0;
                double w_ = 69.898 - 18.67088 * t8;
                double phi = 2 * (w_ - W[5]);
                double chi = 94.9 - 2.292 * t8;
                double a = 24.50601 - 0.08686 * Cos(ToRadians(eta)) - 0.00166 * Cos(ToRadians(zeta + eta)) + 0.00175 * Cos(ToRadians(zeta - eta));
                double e = 0.103458 - 0.004099 * Cos(ToRadians(eta)) - 0.000167 * Cos(ToRadians(zeta + eta))
                    + 0.000235 * Cos(ToRadians(zeta - eta)) + 0.02303 * Cos(ToRadians(zeta)) - 0.00212 * Cos(ToRadians(2 * zeta))
                    + 0.000151 * Cos(ToRadians(3 * zeta)) + 0.00013 * Cos(ToRadians(phi));
                double p = w_ + 0.15648 * Sin(ToRadians(chi)) - 0.4457 * Sin(ToRadians(eta)) - 0.2657 * Sin(ToRadians(zeta + eta))
                    - 0.3573 * Sin(ToRadians(zeta - eta)) - 12.872 * Sin(ToRadians(zeta)) + 1.668 * Sin(ToRadians(2 * zeta))
                    - 0.2419 * Sin(ToRadians(3 * zeta)) - 0.07 * Sin(ToRadians(phi));
                double lambda_ = 177.047 + 16.91993829 * t6 + 0.15648 * Sin(ToRadians(chi)) + 9.142 * Sin(ToRadians(eta))
                    + 0.007 * Sin(ToRadians(2 * eta)) - 0.014 * Sin(ToRadians(3 * eta)) + 0.2275 * Sin(ToRadians(zeta + eta))
                    + 0.2112 * Sin(ToRadians(zeta - eta)) - 0.26 * Sin(ToRadians(zeta)) - 0.0098 * Sin(ToRadians(2 * zeta))
                    - 0.013 * Sin(ToRadians(a_s)) + 0.017 * Sin(ToRadians(b_s)) - 0.0303 * Sin(ToRadians(phi));
                double i = 27.3347 + 0.643486 * Cos(ToRadians(chi)) + 0.315 * Cos(ToRadians(W[3])) + 0.018 * Cos(ToRadians(theta)) - 0.018 * Cos(ToRadians(c_s));
                double Omega = 168.6812 + 1.40136 * Cos(ToRadians(chi)) + 0.68599 * Sin(ToRadians(W[3]))
                    - 0.0392 * Sin(ToRadians(c_s)) + 0.0366 * Sin(ToRadians(theta_));
                Subroutine(e, lambda_, p, Omega, i, a, out lambda[7], out gamma[7], out OMEGA[7], out r[7]);
            }

            // IAPETUS (VIII)
            {
                double L = 261.1582 + 22.57697855 * t4;
                double w__ = 91.796 + 0.562 * t7;
                double psi = 4.367 - 0.195 * t7;
                double theta = 146.819 - 3.198 * t7;
                double phi = 60.470 + 1.521 * t7;
                double PHI = 205.055 - 2.091 * t7;
                double e_ = 0.028298 + 0.001156 * t11;
                double w_0 = 352.91 + 11.71 * t11;
                double mu = 76.3852 + 4.53795125 * t10;
                double i_ = 18.4602 - 0.9518 * t11 - 0.072 * t11 * t11 + 0.0054 * t11 * t11 * t11;
                double OMEGA_ = 143.198 - 3.919 * t11 + 0.116 * t11 * t11 + 0.008 * t11 * t11 * t11;
                double l = mu - w_0;
                double g = w_0 - OMEGA_ - psi;
                double g1 = w_0 - OMEGA_ - phi;
                double ls = W[5] - w__;
                double gs = w__ - theta;
                double lt = L - W[4];
                double gt = W[4] - PHI;
                double u1 = 2 * (l + g - ls - gs);
                double u2 = l + g1 - lt - gt;
                double u3 = l + 2 * (g - ls - gs);
                double u4 = lt + gt - g1;
                double u5 = 2 * (ls + gs);
                double a = 58.935028 + 0.004638 * Cos(ToRadians(u1)) + 0.058222 * Cos(ToRadians(u2));
                double e = e_ - 0.0014097 * Cos(ToRadians(g1 - gt)) + 0.0003733 * Cos(ToRadians(u5 - 2 * g))
                    + 0.0001180 * Cos(ToRadians(u3)) + 0.0002408 * Cos(ToRadians(l))
                    + 0.0003849 * Cos(ToRadians(l + u2)) + 0.0006190 * Cos(ToRadians(u4));
                double w = 0.08077 * Sin(ToRadians(g1 - gt)) + 0.02139 * Sin(ToRadians(u5 - 2 * g)) - 0.00676 * Sin(ToRadians(u3))
                    + 0.01380 * Sin(ToRadians(l)) + 0.01632 * Sin(ToRadians(l + u2)) + 0.03547 * Sin(ToRadians(u4));
                double p = w_0 + w / e_;
                double lambda_ = mu - 0.04299 * Sin(ToRadians(u2)) - 0.00789 * Sin(ToRadians(u1)) - 0.06312 * Sin(ToRadians(ls))
                    - 0.00295 * Sin(ToRadians(2 * ls)) - 0.02231 * Sin(ToRadians(u5)) + 0.00650 * Sin(ToRadians(u5 + psi));
                double i = i_ + 0.04204 * Cos(ToRadians(u5 + psi)) + 0.00235 * Cos(ToRadians(l + g1 + lt + gt + phi))
                    + 0.00360 * Cos(ToRadians(u2 + phi));
                double w_ = 0.04204 * Sin(ToRadians(u5 + psi)) + 0.00235 * Sin(ToRadians(l + g1 + lt + gt + phi))
                    + 0.00358 * Sin(ToRadians(u2 + phi));
                double Omega = OMEGA_ + w_ / Sin(ToRadians(u2 + phi));
                Subroutine(e, lambda_, p, Omega, i, a, out lambda[8], out gamma[8], out OMEGA[8], out r[8]);

            }

            double[] X = new double[10];
            double[] Y = new double[10];
            double[] Z = new double[10];

            for (int j = 1; j <= 8; j++)
            {
                double u = lambda[j] - OMEGA[j];
                double w = OMEGA[j] - 168.8112;
                X[j] = r[j] * (Cos(ToRadians(u)) * Cos(ToRadians(w)) - Sin(ToRadians(u)) * Cos(ToRadians(gamma[j])) * Sin(ToRadians(w)));
                Y[j] = r[j] * (Sin(ToRadians(u)) * Cos(ToRadians(w)) * Cos(ToRadians(gamma[j])) + Cos(ToRadians(u)) * Sin(ToRadians(w)));
                Z[j] = r[j] * Sin(ToRadians(u)) * Sin(ToRadians(gamma[j]));
            }
            X[9] = 0; Y[9] = 0; Z[9] = 1;

            double[] A4 = new double[10];
            double[] B4 = new double[10];
            double[] C4 = new double[10];

            for (int j = 1; j <= 9; j++)
            {
                // Rotation towards the plane of the ecliptic 
                double A1 = X[j];
                double B1 = c1 * Y[j] - s1 * Z[j];
                double C1 = s1 * Y[j] + c1 * Z[j];
                // Rotation towards the vernal equinox
                double A2 = c2 * A1 - s2 * B1;
                double B2 = s2 * A1 + c2 * B1;
                double C2 = C1;

                double A3 = A2 * Sin(ToRadians(e0.Lambda)) - B2 * Cos(ToRadians(e0.Lambda));
                double B3 = A2 * Cos(ToRadians(e0.Lambda)) + B2 * Sin(ToRadians(e0.Lambda));
                double C3 = C2;

                A4[j] = A3;
                B4[j] = B3 * Cos(ToRadians(e0.Beta)) + C3 * Sin(ToRadians(e0.Beta));
                C4[j] = C3 * Cos(ToRadians(e0.Beta)) - B3 * Sin(ToRadians(e0.Beta));
            }

            double D = Atan2(A4[9], C4[9]);

            CrdsRectangular[] moons = new CrdsRectangular[8];

            double[] K = { 20947, 23715, 26382, 29876, 35313, 53800, 59222, 91820 };

            for (int j = 0; j < 8; j++)
            {
                moons[j] = new CrdsRectangular();
                moons[j].X = A4[j + 1] * Cos(D) - C4[j + 1] * Sin(D);
                moons[j].Y = A4[j + 1] * Sin(D) + C4[j + 1] * Cos(D);
                moons[j].Z = B4[j + 1];

                // Light-time effect:
                moons[j].X += Abs(moons[j].Z) / K[j] * Sqrt(1 - Pow((moons[j].X / r[j + 1]), 2));

                // Perspective effect:
                moons[j].X *= (e0.Distance / (e0.Distance + moons[j].Z / 2475.0));
            }

            return moons;
        }

        private static void Subroutine(
            double e,
            double lambda_,
            double p,
            double Omega,
            double i,
            double a,
            out double lambda,
            out double gamma,
            out double w,
            out double r)
        {
            // p. 329 II
            double M = lambda_ - p;
            double e2 = e * e;
            double e3 = e2 * e;
            double e4 = e3 * e;
            double e5 = e4 * e;

            double C = (2 * e - 0.25 * e3 + 0.0520833333 * e5) * Sin(ToRadians(M))
                + (1.25 * e2 - 0.458333333 * e4) * Sin(ToRadians(2 * M))
                + (1.083333333 * e3 - 0.671875 * e5) * Sin(ToRadians(3 * M))
                + 1.072917 * e4 * Sin(ToRadians(4 * M)) + 1.142708 * e5 * Sin(ToRadians(5 * M));

            r = a * (1 - e2) / (1 + e * Cos(ToRadians(M) + C));
            double g = Omega - 168.8112;

            double s1 = Sin(ToRadians(28.0817));
            double c1 = Cos(ToRadians(28.0817));

            double a1 = Sin(ToRadians(i)) * Sin(ToRadians(g));
            double a2 = c1 * Sin(ToRadians(i)) * Cos(ToRadians(g)) - s1 * Cos(ToRadians(i));

            gamma = ToDegrees(Asin(Sqrt(a1 * a1 + a2 * a2)));
            double u = ToDegrees(Atan2(a1, a2));
            w = 168.8112 + u;

            double h = c1 * Sin(ToRadians(i)) - s1 * Cos(ToRadians(i)) * Cos(ToRadians(g));
            double psi = ToDegrees(Atan2(s1 * Sin(ToRadians(g)), h));
            lambda = lambda_ + ToDegrees(C) + u - g - psi;
        }

        /// <summary>
        /// Converts ecliptical coordinates for one equinox to another one 
        /// </summary>
        /// <param name="jd0">Initial epoch (can be J2000 = 2451545.0)</param>
        /// <param name="jd">Target (final) epoch </param>
        /// <returns>Returns ecliptical coordinates for the target epoch</returns>
        /// <remarks>Method is taken from AA(II), p. 134</remarks>
        private static CrdsEcliptical ConvertCoordinatesToEquinox(double jd0, double jd, CrdsEcliptical e0)
        {
            double T = (jd0 - 2451545.0) / 36525.0;
            double t = (jd - jd0) / 36525.0;

            double eta = (47.0029 - 0.06603 * T + 0.000598 * T * T) * t
                + (-0.03302 + 0.000598 * T) * t * t + 0.000060 * t * t * t;

            eta /= 3600.0;

            double Pi = 3289.4789 * T + 0.60622 * T * T
                - (869.8089 + 0.50491 * T) * t + 0.03536 * t * t;

            Pi /= 3600.0;
            Pi += 174.876384;

            double p = (5029.0966 + 2.22226 * T - 0.000042 * T * T) * t
                + (1.11113 - 0.000042 * T) * t * t - 0.000006 * t * t * t;

            p /= 3600.0;

            double A_ = Cos(ToRadians(eta)) * Cos(ToRadians(e0.Beta)) * Sin(ToRadians(Pi - e0.Lambda)) - Sin(ToRadians(eta)) * Sin(ToRadians(e0.Beta));
            double B_ = Cos(ToRadians(e0.Beta)) * Cos(ToRadians(Pi - e0.Lambda));
            double C_ = Cos(ToRadians(eta)) * Sin(ToRadians(e0.Beta)) + Sin(ToRadians(eta)) * Cos(ToRadians(e0.Beta)) * Sin(ToRadians(Pi - e0.Lambda));

            CrdsEcliptical e = new CrdsEcliptical();

            e.Lambda = p + Pi - ToDegrees(Atan2(A_, B_));
            e.Lambda = To360(e.Lambda);

            e.Beta = ToDegrees(Asin(C_));

            e.Distance = e0.Distance;

            return e;
        }
    }
}
