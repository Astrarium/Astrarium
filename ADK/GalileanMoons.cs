using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK
{
    public static class GalileanMoons
    {
        /// <summary>
        /// 1 a.u. (astronomical unit) in km
        /// </summary>
        private const double AU = 149597870;

        /// <summary>
        /// Moons radius, in km
        /// </summary>
        private static readonly double[] MR = { 3643.0 / 2, 3122.0 / 2, 5262.0 / 2, 4821.0 / 2 };

        /// <summary>
        /// Moons orbits semi-major axis, in km
        /// </summary>
        private static readonly double[] A = { 421700, 670900, 1070400, 1882700 };

        /// <summary>
        /// Jupiter radius, in km
        /// </summary>
        private const double JR = 71492;

        /// <summary>
        /// Solar radius, in km 
        /// </summary>
        private const double SR = 6.955e5;

        /// <summary>
        /// Gets visible semidiameter of Galilean moon shadow, in seconds of arc
        /// </summary>
        /// <param name="R">Distance Sun-Jupiter centers, in a.u.</param>
        /// <param name="r">Distance Earth-Jupiter centers, in a.u.</param>
        /// <param name="i">Galilean moon index, from 0 (Io) to 3 (Callisto)</param>
        /// <returns></returns>
        public static double ShadowSemidiameter(double R, double r, int i)
        {
            // moon mean distances from Sun to galilean moons
            double x = R * AU - A[i];

            // distance moon - Jupiter surface
            double y = A[i] - JR;

            double z = (MR[i] * x) / (SR - MR[i]) - y;
            double d = (MR[i] * z) / (y + z);

            return d / JR * PlanetEphem.Semidiameter(5, r);           
        }

        /// <summary>
        /// Gets semidiameter of Galilean moon in seconds of arc
        /// </summary>
        /// <param name="r">Distance Earth-Jupiter centers, in a.u.</param>
        /// <param name="z">Planetocentric z-coordinate of moon, expressed in units of Jupiter equatorial radii, negative if moon is closer to the Earth than Jupiter</param>
        /// <param name="i">Galilean moon index, from 0 (Io) to 3 (Callisto)</param>
        /// <returns></returns>
        public static double MoonSemidiameter(double r, double z, int i)
        {
            return Angle.ToDegrees(Math.Atan2(MR[i], r * AU + z * JR)) * 3600;
        }

        public static CrdsRectangular[,] Positions(double jd, CrdsHeliocentrical earth, CrdsHeliocentrical jupiter)
        {
            CrdsRectangular[,] positions = new CrdsRectangular[4, 2];

            // distance from Earth to Jupiter
            double distance = jupiter.ToRectangular(earth).ToEcliptical().Distance;

            // light-time effect
            double tau = PlanetPositions.LightTimeEffect(distance);

            // time, in days, since calculation epoch, with respect of light-time effect
            double t = jd - 2443000.5 - tau;

            double[] l_deg = new double[5];
            l_deg[1] = 106.07719 + 203.488955790 * t;
            l_deg[2] = 175.73161 + 101.374724735 * t;
            l_deg[3] = 120.55883 + 50.317609207 * t;
            l_deg[4] = 84.44459 + 21.571071177 * t;

            double[] l = new double[5];
            for (int i = 0; i < 5; i++)
            {
                l[i] = Angle.ToRadians(l_deg[i]);
            }

            double[] pi = new double[5];
            pi[1] = Angle.ToRadians(Angle.To360(97.0881 + 0.16138586 * t));
            pi[2] = Angle.ToRadians(Angle.To360(154.8663 + 0.04726307 * t));
            pi[3] = Angle.ToRadians(Angle.To360(188.1840 + 0.00712734 * t));
            pi[4] = Angle.ToRadians(Angle.To360(335.2868 + 0.00184000 * t));

            double[] w = new double[5];
            w[1] = Angle.ToRadians(312.3346 - 0.13279386 * t);
            w[2] = Angle.ToRadians(100.4411 - 0.03263064 * t);
            w[3] = Angle.ToRadians(119.1942 - 0.00717703 * t);
            w[4] = Angle.ToRadians(322.6186 - 0.00175934 * t);

            // Principal inequality in the longitude of Jupiter:
            double GAMMA = 0.33033 * Math.Sin(Angle.ToRadians(163.679 + 0.0010512 * t)) +
                0.03439 * Math.Sin(Angle.ToRadians(34.486 - 0.0161731 * t));

            // Phase of small libraton:
            double PHI_lambda = Angle.ToRadians(199.6766 + 0.17379190 * t);

            // Longitude of the node of the equator of Jupiter on the ecliptic:
            double psi = Angle.ToRadians(316.5182 - 0.00000208 * t);

            // Mean anomalies of Jupiter and Saturn:
            double G = Angle.ToRadians(30.23756 + 0.0830925701 * t + GAMMA);
            double G_ = Angle.ToRadians(31.97853 + 0.0334597339 * t);

            // Longitude of the perihelion of Jupiter:
            double Pi = Angle.ToRadians(13.469942);

            double[] SIGMA = new double[5];

            SIGMA[1] =
                   0.47259 * Math.Sin(2 * (l[1] - l[2])) +
                  -0.03478 * Math.Sin(pi[3] - pi[4]) +
                  0.01081 * Math.Sin(l[2] - 2 * l[3] + pi[3]) +
                  0.00738 * Math.Sin(PHI_lambda) +
                  0.00713 * Math.Sin(l[2] - 2 * l[3] + pi[2]) +
                  -0.00674 * Math.Sin(pi[1] + pi[3] - 2 * Pi - 2 * G) +
                  0.00666 * Math.Sin(l[2] - 2 * l[3] + pi[4]) +
                  0.00445 * Math.Sin(l[1] - pi[3]) +
                  -0.00354 * Math.Sin(l[1] - l[2]) +
                  -0.00317 * Math.Sin(2 * psi - 2 * Pi) +
                  0.00265 * Math.Sin(l[1] - pi[4]) +
                  -0.00186 * Math.Sin(G) +
                  0.00162 * Math.Sin(pi[2] - pi[3]) +
                  0.00158 * Math.Sin(4 * (l[1] - l[2])) +
                  -0.00155 * Math.Sin(l[1] - l[3]) +
                  -0.00138 * Math.Sin(psi + w[3] - 2 * Pi - 2 * G) +
                  -0.00115 * Math.Sin(2 * (l[1] - 2 * l[2] + w[2])) +
                  0.00089 * Math.Sin(pi[2] - pi[4]) +
                  0.00085 * Math.Sin(l[1] + pi[3] - 2 * Pi - 2 * G) +
                  0.00083 * Math.Sin(w[2] - w[3]) +
                  0.00053 * Math.Sin(psi - w[2]);
            SIGMA[2] =
                  1.06476 * Math.Sin(2 * (l[2] - l[3])) +
                  0.04256 * Math.Sin(l[1] - 2 * l[2] + pi[3]) +
                  0.03581 * Math.Sin(l[2] - pi[3]) +
                  0.02395 * Math.Sin(l[1] - 2 * l[2] + pi[4]) +
                  0.01984 * Math.Sin(l[2] - pi[4]) +
                  -0.01778 * Math.Sin(PHI_lambda) +
                  0.01654 * Math.Sin(l[2] - pi[2]) +
                  0.01334 * Math.Sin(l[2] - 2 * l[3] + pi[2]) +
                  0.01294 * Math.Sin(pi[3] - pi[4]) +
                  -0.01142 * Math.Sin(l[2] - l[3]) +
                  -0.01057 * Math.Sin(G) +
                  -0.00775 * Math.Sin(2 * (psi - Pi)) +
                  0.00524 * Math.Sin(2 * (l[1] - l[2])) +
                  -0.00460 * Math.Sin(l[1] - l[3]) +
                  0.00316 * Math.Sin(psi - 2 * G + w[3] - 2 * Pi) +
                  -0.00203 * Math.Sin(pi[1] + pi[3] - 2 * Pi - 2 * G) +
                  0.00146 * Math.Sin(psi - w[3]) +
                  -0.00145 * Math.Sin(2 * G) +
                  0.00125 * Math.Sin(psi - w[4]) +
                  -0.00115 * Math.Sin(l[1] - 2 * l[3] + pi[3]) +
                  -0.00094 * Math.Sin(2 * (l[2] - w[2])) +
                  0.00086 * Math.Sin(2 * (l[1] - 2 * l[2] + w[2])) +
                  -0.00086 * Math.Sin(5 * G_ - 2 * G + Angle.ToRadians(52.225)) +
                  -0.00078 * Math.Sin(l[2] - l[4]) +
                  -0.00064 * Math.Sin(3 * l[3] - 7 * l[4] + 4 * pi[4]) +
                  0.00064 * Math.Sin(pi[1] - pi[4]) +
                  -0.00063 * Math.Sin(l[1] - 2 * l[3] + pi[4]) +
                  0.00058 * Math.Sin(w[3] - w[4]) +
                  0.00056 * Math.Sin(2 * (psi - Pi - G)) +
                  0.00056 * Math.Sin(2 * (l[2] - l[4])) +
                  0.00055 * Math.Sin(2 * (l[1] - l[3])) +
                  0.00052 * Math.Sin(3 * l[3] - 7 * l[4] + pi[3] + 3 * pi[4]) +
                  -0.00043 * Math.Sin(l[1] - pi[3]) +
                  0.00041 * Math.Sin(5 * (l[2] - l[3])) +
                  0.00041 * Math.Sin(pi[4] - Pi) +
                  0.00032 * Math.Sin(w[2] - w[3]) +
                  0.00032 * Math.Sin(2 * (l[3] - G - Pi));
            SIGMA[3] =
                    0.16490 * Math.Sin(l[3] - pi[3]) +
                  0.09081 * Math.Sin(l[3] - pi[4]) +
                  -0.06907 * Math.Sin(l[2] - l[3]) +
                  0.03784 * Math.Sin(pi[3] - pi[4]) +
                  0.01846 * Math.Sin(2 * (l[3] - l[4])) +
                  -0.01340 * Math.Sin(G) +
                  -0.01014 * Math.Sin(2 * (psi - Pi)) +
                  0.00704 * Math.Sin(l[2] - 2 * l[3] + pi[3]) +
                  -0.00620 * Math.Sin(l[2] - 2 * l[3] + pi[2]) +
                  -0.00541 * Math.Sin(l[3] - l[4]) +
                  0.00381 * Math.Sin(l[2] - 2 * l[3] + pi[4]) +
                  0.00235 * Math.Sin(psi - w[3]) +
                  0.00198 * Math.Sin(psi - w[4]) +
                  0.00176 * Math.Sin(PHI_lambda) +
                  0.00130 * Math.Sin(3 * (l[3] - l[4])) +
                  0.00125 * Math.Sin(l[1] - l[3]) +
                  -0.00119 * Math.Sin(5 * G_ - 2 * G + Angle.ToRadians(52.225)) +
                  0.00109 * Math.Sin(l[1] - l[2]) +
                  -0.00100 * Math.Sin(3 * l[3] - 7 * l[4] + 4 * pi[4]) +
                  0.00091 * Math.Sin(w[3] - w[4]) +
                  0.00080 * Math.Sin(3 * l[3] - 7 * l[4] + pi[3] + 3 * pi[4]) +
                  -0.00075 * Math.Sin(2 * l[2] - 3 * l[3] + pi[3]) +
                  0.00072 * Math.Sin(pi[1] + pi[3] - 2 * Pi - 2 * G) +
                  0.00069 * Math.Sin(pi[4] - Pi) +
                  -0.00058 * Math.Sin(2 * l[3] - 3 * l[4] + pi[4]) +
                  -0.00057 * Math.Sin(l[3] - 2 * l[4] + pi[4]) +
                  0.00056 * Math.Sin(l[3] + pi[3] - 2 * Pi - 2 * G) +
                  -0.00052 * Math.Sin(l[2] - 2 * l[3] + pi[1]) +
                  -0.00050 * Math.Sin(pi[2] - pi[3]) +
                  0.00048 * Math.Sin(l[3] - 2 * l[4] + pi[3]) +
                  -0.00045 * Math.Sin(2 * l[2] - 3 * l[3] + pi[4]) +
                  -0.00041 * Math.Sin(pi[2] - pi[4]) +
                  -0.00038 * Math.Sin(2 * G) +
                  -0.00037 * Math.Sin(pi[3] - pi[4] + w[3] - w[4]) +
                  -0.00032 * Math.Sin(3 * l[3] - 7 * l[4] + 2 * pi[3] + 2 * pi[4]) +
                  0.00030 * Math.Sin(4 * (l[3] - l[4])) +
                  0.00029 * Math.Sin(l[3] + pi[4] - 2 * Pi - 2 * G) +
                  -0.00028 * Math.Sin(w[3] + psi - 2 * Pi - 2 * G) +
                  0.00026 * Math.Sin(l[3] - Pi - G) +
                  0.00024 * Math.Sin(l[2] - 3 * l[3] + 2 * l[4]) +
                  0.00021 * Math.Sin(l[3] - Pi - G) +
                  -0.00021 * Math.Sin(l[3] - pi[2]) +
                  0.00017 * Math.Sin(2 * (l[3] - pi[3]));
            SIGMA[4] =
                0.84287 * Math.Sin(l[4] - pi[4]) +
                  0.03431 * Math.Sin(pi[4] - pi[3]) +
                  -0.03305 * Math.Sin(2 * (psi - Pi)) +
                  -0.03211 * Math.Sin(G) +
                  -0.01862 * Math.Sin(l[4] - pi[3]) +
                  0.01186 * Math.Sin(psi - w[4]) +
                  0.00623 * Math.Sin(l[4] + pi[4] - 2 * G - 2 * Pi) +
                  0.00387 * Math.Sin(2 * (l[4] - pi[4])) +
                  -0.00284 * Math.Sin(5 * G_ - 2 * G + Angle.ToRadians(52.225)) +
                  -0.00234 * Math.Sin(2 * (psi - pi[4])) +
                  -0.00223 * Math.Sin(l[3] - l[4]) +
                  -0.00208 * Math.Sin(l[4] - Pi) +
                  0.00178 * Math.Sin(psi + w[4] - 2 * pi[4]) +
                  0.00134 * Math.Sin(pi[4] - Pi) +
                  0.00125 * Math.Sin(2 * (l[4] - G - Pi)) +
                  -0.00117 * Math.Sin(2 * G) +
                  -0.00112 * Math.Sin(2 * (l[3] - l[4])) +
                  0.00107 * Math.Sin(3 * l[3] - 7 * l[4] + 4 * pi[4]) +
                  0.00102 * Math.Sin(l[4] - G - Pi) +
                  0.00096 * Math.Sin(2 * l[4] - psi - w[4]) +
                  0.00087 * Math.Sin(2 * (psi - w[4])) +
                  -0.00085 * Math.Sin(3 * l[3] - 7 * l[4] + pi[3] + 3 * pi[4]) +
                  0.00085 * Math.Sin(l[3] - 2 * l[4] + pi[4]) +
                  -0.00081 * Math.Sin(2 * (l[4] - psi)) +
                  0.00071 * Math.Sin(l[4] + pi[4] - 2 * Pi - 3 * G) +
                  0.00061 * Math.Sin(l[1] - l[4]) +
                  -0.00056 * Math.Sin(psi - w[3]) +
                  -0.00054 * Math.Sin(l[3] - 2 * l[4] + pi[3]) +
                  0.00051 * Math.Sin(l[2] - l[4]) +
                  0.00042 * Math.Sin(2 * (psi - G - Pi)) +
                  0.00039 * Math.Sin(2 * (pi[4] - w[4])) +
                  0.00036 * Math.Sin(psi + Pi - pi[4] - w[4]) +
                  0.00035 * Math.Sin(2 * G_ - G + Angle.ToRadians(188.37)) +
                  -0.00035 * Math.Sin(l[4] - pi[4] + 2 * Pi - 2 * psi) +
                  -0.00032 * Math.Sin(l[4] + pi[4] - 2 * Pi - G) +
                  0.00030 * Math.Sin(2 * G_ - 2 * G + Angle.ToRadians(149.15)) +
                  0.00029 * Math.Sin(3 * l[3] - 7 * l[4] + 2 * pi[3] + 2 * pi[4]) +
                  0.00028 * Math.Sin(l[4] - pi[4] + 2 * psi - 2 * Pi) +
                  -0.00028 * Math.Sin(2 * (l[4] - w[4])) +
                  -0.00027 * Math.Sin(pi[3] - pi[4] + w[3] - w[4]) +
                  -0.00026 * Math.Sin(5 * G_ - 3 * G + Angle.ToRadians(188.37)) +
                  0.00025 * Math.Sin(w[4] - w[3]) +
                  -0.00025 * Math.Sin(l[2] - 3 * l[3] + 2 * l[4]) +
                  -0.00023 * Math.Sin(3 * (l[3] - l[4])) +
                  0.00021 * Math.Sin(2 * l[4] - 2 * Pi - 3 * G) +
                  -0.00021 * Math.Sin(2 * l[3] - 3 * l[4] + pi[4]) +
                  0.00019 * Math.Sin(l[4] - pi[4] - G) +
                  -0.00019 * Math.Sin(2 * l[4] - pi[3] - pi[4]) +
                  -0.00018 * Math.Sin(l[4] - pi[4] + G) +
                  -0.00016 * Math.Sin(l[4] + pi[3] - 2 * Pi - 2 * G);

            // True longitudes of the sattelites:
            double[] L = new double[5];
            for (int i = 0; i < 5; i++)
            {
                L[i] = Angle.ToRadians(Angle.To360(l_deg[i] + SIGMA[i]));
                SIGMA[i] = Angle.ToRadians(SIGMA[i]);
            }

            double[] BB = new double[5];

            BB[1] = Math.Atan(
                0.0006393 * Math.Sin(L[1] - w[1]) +
                0.0001825 * Math.Sin(L[1] - w[2]) +
                0.0000329 * Math.Sin(L[1] - w[3]) +
                -0.0000311 * Math.Sin(L[1] - psi) +
                0.0000093 * Math.Sin(L[1] - w[4]) +
                0.0000075 * Math.Sin(3 * L[1] - 4 * l[2] - 1.9927 * SIGMA[1] + w[2]) +
                0.0000046 * Math.Sin(L[1] + psi - 2 * Pi - 2 * G));

            BB[2] = Math.Atan(
                0.0081004 * Math.Sin(L[2] - w[2]) +
                0.0004512 * Math.Sin(L[2] - w[3]) +
                -0.0003284 * Math.Sin(L[2] - psi) +
                0.0001160 * Math.Sin(L[2] - w[4]) +
                0.0000272 * Math.Sin(l[1] - 2 * l[3] + 1.0146 * SIGMA[2] + w[2]) +
                -0.0000144 * Math.Sin(L[2] - w[1]) +
                0.0000143 * Math.Sin(L[2] + psi - 2 * Pi - 2 * G) +
                0.0000035 * Math.Sin(L[2] - psi + G) +
                -0.0000028 * Math.Sin(l[1] - 2 * l[3] + 1.0146 * SIGMA[2] + w[3]));
            BB[3] = Math.Atan(
                0.0032402 * Math.Sin(L[3] - w[3]) +
                -0.0016911 * Math.Sin(L[3] - psi) +
                0.0006847 * Math.Sin(L[3] - w[4]) +
                -0.0002797 * Math.Sin(L[3] - w[2]) +
                0.0000321 * Math.Sin(L[3] + psi - 2 * Pi - 2 * G) +
                0.0000051 * Math.Sin(L[3] - psi + G) +
                -0.0000045 * Math.Sin(L[3] - psi - G) +
                -0.0000045 * Math.Sin(L[3] + psi - 2 * Pi) +
                0.0000037 * Math.Sin(L[3] + psi - 2 * Pi - 3 * G) +
                0.0000030 * Math.Sin(2 * l[2] - 3 * L[3] + 4.03 * SIGMA[3] + w[2]) +
                -0.0000021 * Math.Sin(2 * l[2] - 3 * L[3] + 4.03 * SIGMA[3] + w[3]));

            BB[4] = Math.Atan(
                -0.0076579 * Math.Sin(L[4] - psi) +
                0.0044134 * Math.Sin(L[4] - w[4]) +
                -0.0005112 * Math.Sin(L[4] - w[3]) +
                0.0000773 * Math.Sin(L[4] + psi - 2 * Pi - 2 * G) +
                0.0000104 * Math.Sin(L[4] - psi + G) +
                -0.0000102 * Math.Sin(L[4] - psi - G) +
                0.0000088 * Math.Sin(L[4] + psi - 2 * Pi - 3 * G) +
                -0.0000038 * Math.Sin(L[4] + psi - 2 * Pi - G));

            double[] R = new double[5];
            R[1] =
                5.90569 * (1 + (-0.0041339 * Math.Cos(2 * (l[1] - l[2])) +
                -0.0000387 * Math.Cos(l[1] - pi[3]) +
                -0.0000214 * Math.Cos(l[1] - pi[4]) +
                0.0000170 * Math.Cos(l[1] - l[2]) +
                -0.0000131 * Math.Cos(4 * (l[1] - l[2])) +
                0.0000106 * Math.Cos(l[1] - l[3]) +
                -0.0000066 * Math.Cos(l[1] + pi[3] - 2 * Pi - 2 * G)));
            R[2] =
                9.39657 * (1 + (0.0093848 * Math.Cos(l[1] - l[2]) +
                -0.0003116 * Math.Cos(l[2] - pi[3]) +
                -0.0001744 * Math.Cos(l[2] - pi[4]) +
                -0.0001442 * Math.Cos(l[2] - pi[2]) +
                0.0000553 * Math.Cos(l[2] - l[3]) +
                0.0000523 * Math.Cos(l[1] - l[3]) +
                -0.0000290 * Math.Cos(2 * (l[1] - l[2])) +
                0.0000164 * Math.Cos(2 * (l[2] - w[2])) +
                0.0000107 * Math.Cos(l[1] - 2 * l[3] + pi[3]) +
                -0.0000102 * Math.Cos(l[2] - pi[1]) +
                -0.0000091 * Math.Cos(2 * (l[1] - l[3]))));
            R[3] =
                14.98832 * (1 + (-0.0014388 * Math.Cos(l[3] - pi[3]) +
                -0.0007919 * Math.Cos(l[3] - pi[4]) +
                0.0006342 * Math.Cos(l[2] - l[3]) +
                -0.0001761 * Math.Cos(2 * (l[3] - l[4])) +
                0.0000294 * Math.Cos(l[3] - l[4]) +
                -0.0000156 * Math.Cos(3 * (l[3] - l[4])) +
                0.0000156 * Math.Cos(l[1] - l[3]) +
                -0.0000153 * Math.Cos(l[1] - l[2]) +
                0.0000070 * Math.Cos(2 * l[2] - 3 * l[3] + pi[3]) +
                -0.0000051 * Math.Cos(l[3] + pi[3] - 2 * Pi - 2 * G)));
            R[4] =
                26.36273 * (1 + (-0.0073546 * Math.Cos(l[4] - pi[4]) +
                0.0001621 * Math.Cos(l[4] - pi[3]) +
                0.0000974 * Math.Cos(l[3] - l[4]) +
                -0.0000543 * Math.Cos(l[4] + pi[4] - 2 * Pi - 2 * G) +
                -0.0000271 * Math.Cos(2 * (l[4] - pi[4])) +
                0.0000182 * Math.Cos(l[4] - Pi) +
                0.0000177 * Math.Cos(2 * (l[3] - l[4])) +
                -0.0000167 * Math.Cos(2 * l[4] - psi - w[4]) +
                0.0000167 * Math.Cos(psi - w[4]) +
                -0.0000155 * Math.Cos(2 * (l[4] - Pi - G)) +
                0.0000142 * Math.Cos(2 * (l[4] - psi)) +
                0.0000105 * Math.Cos(l[1] - l[4]) +
                0.0000092 * Math.Cos(l[2] - l[4]) +
                -0.0000089 * Math.Cos(l[4] - Pi - G) +
                -0.0000062 * Math.Cos(l[4] + pi[4] - 2 * Pi - 3 * G) +
                0.0000048 * Math.Cos(2 * (l[4] - w[4]))));

            double T0 = (jd - 2433282.423) / 36525.0;
            double P = Angle.ToRadians(1.3966626 * T0 + 0.0003088 * T0 * T0);

            for (int i = 0; i < 5; i++)
            {
                L[i] += P;
            }
            psi += P;

            double T = (jd - 2415020.5) / 36525;
            double I = Angle.ToRadians(3.120262 + 0.0006 * T);

            double[] X = new double[6];
            double[] Y = new double[6];
            double[] Z = new double[6];

            for (int i = 1; i < 5; i++)
            {
                X[i] = R[i] * Math.Cos(L[i] - psi) * Math.Cos(BB[i]);
                Y[i] = R[i] * Math.Sin(L[i] - psi) * Math.Cos(BB[i]);
                Z[i] = R[i] * Math.Sin(BB[i]);
            }

            X[5] = 0; Y[5] = 0; Z[5] = 1;

            double[] A1 = new double[6];
            double[] B1 = new double[6];
            double[] C1 = new double[6];

            for (int i = 1; i < 6; i++)
            {
                A1[i] = X[i];
                B1[i] = Y[i] * Math.Cos(I) - Z[i] * Math.Sin(I);
                C1[i] = Y[i] * Math.Sin(I) + Z[i] * Math.Cos(I);
            }

            double[] A2 = new double[6];
            double[] B2 = new double[6];
            double[] C2 = new double[6];

            double T1 = (jd - 2451545.0) / 36525;
            double T2 = T1 * T1;
            double T3 = T2 * T1;

            double OMEGA = 100.464407 + 1.0209774 * T1 + 0.00040315 * T2 + 0.000000404 * T3;
            OMEGA = Angle.ToRadians(OMEGA);

            double Inc = 1.303267 - 0.0054965 * T1 + 0.00000466 * T2 + 0.000000002 * T3;
            Inc = Angle.ToRadians(Inc);

            double PHI = psi - OMEGA;

            for (int i = 5; i >= 1; i--)
            {
                A2[i] = A1[i] * Math.Cos(PHI) - B1[i] * Math.Sin(PHI);
                B2[i] = A1[i] * Math.Sin(PHI) + B1[i] * Math.Cos(PHI);
                C2[i] = C1[i];
            }

            double[] A3 = new double[6];
            double[] B3 = new double[6];
            double[] C3 = new double[6];

            for (int i = 5; i >= 1; i--)
            {
                A3[i] = A2[i];
                B3[i] = B2[i] * Math.Cos(Inc) - C2[i] * Math.Sin(Inc);
                C3[i] = B2[i] * Math.Sin(Inc) + C2[i] * Math.Cos(Inc);
            }

            double[] A4 = new double[6];
            double[] B4 = new double[6];
            double[] C4 = new double[6];

            for (int i = 5; i >= 1; i--)
            {
                A4[i] = A3[i] * Math.Cos(OMEGA) - B3[i] * Math.Sin(OMEGA);
                B4[i] = A3[i] * Math.Sin(OMEGA) + B3[i] * Math.Cos(OMEGA);
                C4[i] = C3[i];
            }

            double[] A5 = new double[6];
            double[] B5 = new double[6];
            double[] C5 = new double[6];

            for (int m = 0; m < 2; m++)
            {
                // "0" for shadows
                double Radius = m == 0 ? earth.R : 0; 

                // Rectangular geocentric ecliptic coordinates of Jupiter:
                double x = jupiter.R * Math.Cos(Angle.ToRadians(jupiter.B)) * Math.Cos(Angle.ToRadians(jupiter.L)) + Radius * Math.Cos(Angle.ToRadians(earth.L + 180));
                double y = jupiter.R * Math.Cos(Angle.ToRadians(jupiter.B)) * Math.Sin(Angle.ToRadians(jupiter.L)) + Radius * Math.Sin(Angle.ToRadians(earth.L + 180));
                double z = jupiter.R * Math.Sin(Angle.ToRadians(jupiter.B)) + Radius * Math.Sin(Angle.ToRadians(-earth.B));

                double Delta = Math.Sqrt(x * x + y * y + z * z);
                double LAMBDA = Math.Atan2(y, x);
                double alpha = Math.Atan(z / Math.Sqrt(x * x + y * y));

                for (int i = 5; i >= 1; i--)
                {
                    A5[i] = A4[i] * Math.Sin(LAMBDA) - B4[i] * Math.Cos(LAMBDA);
                    B5[i] = A4[i] * Math.Cos(LAMBDA) + B4[i] * Math.Sin(LAMBDA);
                    C5[i] = C4[i];
                }

                double[] A6 = new double[6];
                double[] B6 = new double[6];
                double[] C6 = new double[6];

                for (int i = 5; i >= 1; i--)
                {
                    A6[i] = A5[i];
                    B6[i] = C5[i] * Math.Sin(alpha) + B5[i] * Math.Cos(alpha);
                    C6[i] = C5[i] * Math.Cos(alpha) - B5[i] * Math.Sin(alpha);
                }

                double D = Math.Atan2(A6[5], C6[5]);

                CrdsRectangular[] rectangular = new CrdsRectangular[4];

                for (int i = 0; i < 4; i++)
                {
                    rectangular[i] = new CrdsRectangular(
                        A6[i + 1] * Math.Cos(D) - C6[i + 1] * Math.Sin(D),
                        A6[i + 1] * Math.Sin(D) + C6[i + 1] * Math.Cos(D),
                        B6[i + 1]
                    );
                }

                double[] K = { 17295, 21819, 27558, 36548 };

                for (int i = 0; i < 4; i++)
                {
                    rectangular[i].X += Math.Abs(rectangular[i].Z) / K[i] * Math.Sqrt(1 - Math.Pow(rectangular[i].X / R[i + 1], 2));
                }

                for (int i = 0; i < 4; i++)
                {
                    double W = Delta / (Delta + rectangular[i].Z / 2095.0);
                    rectangular[i].X *= W;
                    rectangular[i].Y *= W;
                }

                for (int i = 0; i < 4; i++)
                {
                    positions[i, m] = rectangular[i];
                }
            }

            return positions;
        }
    }
}
