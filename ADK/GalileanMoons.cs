using System;
using static System.Math;
using static ADK.Angle;
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
        /// Jupiter radius, in km
        /// </summary>
        private const double JR = 71492;

        /// <summary>
        /// Solar radius, in km 
        /// </summary>
        private const double SR = 6.955e5;

        public static GalileanMoonShadowAppearance Shadow(double distanceFromEarth, double distanceFromSun, int moonIndex, CrdsRectangular moon, CrdsRectangular eclipsedBody)
        {
            // distance between bodies, in km
            double d = Sqrt(Pow(moon.X - eclipsedBody.X, 2) + Pow(moon.Y - eclipsedBody.Y, 2) + Pow(moon.Z - eclipsedBody.Z, 2)) * JR;

            // distance between Sun and moon
            double D = 
                // distance from Sun to Jupiter, in km
                distanceFromSun * AU 
                // distance from Jupiter to moon, projected on the light direction
                + moon.Z * JR;

            return Shadow(MR[moonIndex], D, d, distanceFromEarth);
        }

        /// <summary>
        /// Calculates appearance of shadow
        /// </summary>
        /// <param name="r">radius (in km) of the moon that casts the shadow</param>
        /// <param name="D">distance (in km) from light source (Sun) to the moon that casts the shadow</param>
        /// <param name="d">distance (in km) from the moon that casts the shadow to eclipsed body surface (projection plane)</param>
        /// <param name="d0">distance (in km) from Earth to projection plane</param>
        private static GalileanMoonShadowAppearance Shadow(double r, double D, double d, double d0)
        {
            // Focal distance, in km, is a distance from light source (i.e. Sun) to focal point.
            // Focal point is a vertex of shadow cone, where moon shadow decreases to the single point.
            double F = D * SR / (SR - r);

            // Tangent of angle theta. 
            // Theta angle is a visible semidiameter of the Sun from focal point
            double tanTheta = SR / F;

            // Distance from focal point to eclipsed body surface (projection plane), in km
            double f = F - D - d;

            // Umbra radius on the eclipsed body surface (projection plane), in km
            double u = f * tanTheta;

            // F0 - is an intercrossing point
            // It's a point between light source (Sun) and the moon that casts the shadow
            // where sun rays from top and bottom of solar disk cross each other
            // So, T + t = D

            // Distance from the light source center (Sun) to intercrossing point, in km
            double T = D / (1 + r / SR);

            // Distance from intercrossing point to the moon that casts the shadow, in km
            double t = D - T;

            // Penumbra radius on the eclipsed body surface (projection plane), in km
            double p = SR / T * (t + d);

            return new GalileanMoonShadowAppearance()
            {
                Umbra = ToDegrees(Atan2(u, d0 * AU)) * 3600,
                Penumbra = ToDegrees(Atan2(p, d0 * AU)) * 3600,
            };
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
            return ToDegrees(Atan2(MR[i], r * AU + z * JR)) * 3600;
        }

        /// <summary>
        /// Gets longitude of central meridian of Galilean moon
        /// </summary>
        /// <param name="r">Planetocentric rectangular coordinates of the moon</param>
        /// <param name="i">Galilean moon index, from 0 (Io) to 3 (Callisto)</param>
        /// <returns></returns>
        public static double MoonCentralMeridian(CrdsRectangular r, int i)
        {
            // distance from Juputer, in Jupiter equatorial radii
            double distance = Sqrt(r.X * r.X + r.Y * r.Y + r.Z * r.Z);

            return To360(ToDegrees(Atan2(r.Z / distance, r.X / distance)) + 270);
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
                l[i] = ToRadians(l_deg[i]);
            }

            double[] pi = new double[5];
            pi[1] = ToRadians(To360(97.0881 + 0.16138586 * t));
            pi[2] = ToRadians(To360(154.8663 + 0.04726307 * t));
            pi[3] = ToRadians(To360(188.1840 + 0.00712734 * t));
            pi[4] = ToRadians(To360(335.2868 + 0.00184000 * t));

            double[] w = new double[5];
            w[1] = ToRadians(312.3346 - 0.13279386 * t);
            w[2] = ToRadians(100.4411 - 0.03263064 * t);
            w[3] = ToRadians(119.1942 - 0.00717703 * t);
            w[4] = ToRadians(322.6186 - 0.00175934 * t);

            // Principal inequality in the longitude of Jupiter:
            double GAMMA = 0.33033 * Sin(ToRadians(163.679 + 0.0010512 * t)) +
                0.03439 * Sin(ToRadians(34.486 - 0.0161731 * t));

            // Phase of small libraton:
            double PHI_lambda = ToRadians(199.6766 + 0.17379190 * t);

            // Longitude of the node of the equator of Jupiter on the ecliptic:
            double psi = ToRadians(316.5182 - 0.00000208 * t);

            // Mean anomalies of Jupiter and Saturn:
            double G = ToRadians(30.23756 + 0.0830925701 * t + GAMMA);
            double G_ = ToRadians(31.97853 + 0.0334597339 * t);

            // Longitude of the perihelion of Jupiter:
            double Pi = ToRadians(13.469942);

            double[] SIGMA = new double[5];

            SIGMA[1] =
                   0.47259 * Sin(2 * (l[1] - l[2])) +
                  -0.03478 * Sin(pi[3] - pi[4]) +
                  0.01081 * Sin(l[2] - 2 * l[3] + pi[3]) +
                  0.00738 * Sin(PHI_lambda) +
                  0.00713 * Sin(l[2] - 2 * l[3] + pi[2]) +
                  -0.00674 * Sin(pi[1] + pi[3] - 2 * Pi - 2 * G) +
                  0.00666 * Sin(l[2] - 2 * l[3] + pi[4]) +
                  0.00445 * Sin(l[1] - pi[3]) +
                  -0.00354 * Sin(l[1] - l[2]) +
                  -0.00317 * Sin(2 * psi - 2 * Pi) +
                  0.00265 * Sin(l[1] - pi[4]) +
                  -0.00186 * Sin(G) +
                  0.00162 * Sin(pi[2] - pi[3]) +
                  0.00158 * Sin(4 * (l[1] - l[2])) +
                  -0.00155 * Sin(l[1] - l[3]) +
                  -0.00138 * Sin(psi + w[3] - 2 * Pi - 2 * G) +
                  -0.00115 * Sin(2 * (l[1] - 2 * l[2] + w[2])) +
                  0.00089 * Sin(pi[2] - pi[4]) +
                  0.00085 * Sin(l[1] + pi[3] - 2 * Pi - 2 * G) +
                  0.00083 * Sin(w[2] - w[3]) +
                  0.00053 * Sin(psi - w[2]);
            SIGMA[2] =
                  1.06476 * Sin(2 * (l[2] - l[3])) +
                  0.04256 * Sin(l[1] - 2 * l[2] + pi[3]) +
                  0.03581 * Sin(l[2] - pi[3]) +
                  0.02395 * Sin(l[1] - 2 * l[2] + pi[4]) +
                  0.01984 * Sin(l[2] - pi[4]) +
                  -0.01778 * Sin(PHI_lambda) +
                  0.01654 * Sin(l[2] - pi[2]) +
                  0.01334 * Sin(l[2] - 2 * l[3] + pi[2]) +
                  0.01294 * Sin(pi[3] - pi[4]) +
                  -0.01142 * Sin(l[2] - l[3]) +
                  -0.01057 * Sin(G) +
                  -0.00775 * Sin(2 * (psi - Pi)) +
                  0.00524 * Sin(2 * (l[1] - l[2])) +
                  -0.00460 * Sin(l[1] - l[3]) +
                  0.00316 * Sin(psi - 2 * G + w[3] - 2 * Pi) +
                  -0.00203 * Sin(pi[1] + pi[3] - 2 * Pi - 2 * G) +
                  0.00146 * Sin(psi - w[3]) +
                  -0.00145 * Sin(2 * G) +
                  0.00125 * Sin(psi - w[4]) +
                  -0.00115 * Sin(l[1] - 2 * l[3] + pi[3]) +
                  -0.00094 * Sin(2 * (l[2] - w[2])) +
                  0.00086 * Sin(2 * (l[1] - 2 * l[2] + w[2])) +
                  -0.00086 * Sin(5 * G_ - 2 * G + ToRadians(52.225)) +
                  -0.00078 * Sin(l[2] - l[4]) +
                  -0.00064 * Sin(3 * l[3] - 7 * l[4] + 4 * pi[4]) +
                  0.00064 * Sin(pi[1] - pi[4]) +
                  -0.00063 * Sin(l[1] - 2 * l[3] + pi[4]) +
                  0.00058 * Sin(w[3] - w[4]) +
                  0.00056 * Sin(2 * (psi - Pi - G)) +
                  0.00056 * Sin(2 * (l[2] - l[4])) +
                  0.00055 * Sin(2 * (l[1] - l[3])) +
                  0.00052 * Sin(3 * l[3] - 7 * l[4] + pi[3] + 3 * pi[4]) +
                  -0.00043 * Sin(l[1] - pi[3]) +
                  0.00041 * Sin(5 * (l[2] - l[3])) +
                  0.00041 * Sin(pi[4] - Pi) +
                  0.00032 * Sin(w[2] - w[3]) +
                  0.00032 * Sin(2 * (l[3] - G - Pi));
            SIGMA[3] =
                    0.16490 * Sin(l[3] - pi[3]) +
                  0.09081 * Sin(l[3] - pi[4]) +
                  -0.06907 * Sin(l[2] - l[3]) +
                  0.03784 * Sin(pi[3] - pi[4]) +
                  0.01846 * Sin(2 * (l[3] - l[4])) +
                  -0.01340 * Sin(G) +
                  -0.01014 * Sin(2 * (psi - Pi)) +
                  0.00704 * Sin(l[2] - 2 * l[3] + pi[3]) +
                  -0.00620 * Sin(l[2] - 2 * l[3] + pi[2]) +
                  -0.00541 * Sin(l[3] - l[4]) +
                  0.00381 * Sin(l[2] - 2 * l[3] + pi[4]) +
                  0.00235 * Sin(psi - w[3]) +
                  0.00198 * Sin(psi - w[4]) +
                  0.00176 * Sin(PHI_lambda) +
                  0.00130 * Sin(3 * (l[3] - l[4])) +
                  0.00125 * Sin(l[1] - l[3]) +
                  -0.00119 * Sin(5 * G_ - 2 * G + ToRadians(52.225)) +
                  0.00109 * Sin(l[1] - l[2]) +
                  -0.00100 * Sin(3 * l[3] - 7 * l[4] + 4 * pi[4]) +
                  0.00091 * Sin(w[3] - w[4]) +
                  0.00080 * Sin(3 * l[3] - 7 * l[4] + pi[3] + 3 * pi[4]) +
                  -0.00075 * Sin(2 * l[2] - 3 * l[3] + pi[3]) +
                  0.00072 * Sin(pi[1] + pi[3] - 2 * Pi - 2 * G) +
                  0.00069 * Sin(pi[4] - Pi) +
                  -0.00058 * Sin(2 * l[3] - 3 * l[4] + pi[4]) +
                  -0.00057 * Sin(l[3] - 2 * l[4] + pi[4]) +
                  0.00056 * Sin(l[3] + pi[3] - 2 * Pi - 2 * G) +
                  -0.00052 * Sin(l[2] - 2 * l[3] + pi[1]) +
                  -0.00050 * Sin(pi[2] - pi[3]) +
                  0.00048 * Sin(l[3] - 2 * l[4] + pi[3]) +
                  -0.00045 * Sin(2 * l[2] - 3 * l[3] + pi[4]) +
                  -0.00041 * Sin(pi[2] - pi[4]) +
                  -0.00038 * Sin(2 * G) +
                  -0.00037 * Sin(pi[3] - pi[4] + w[3] - w[4]) +
                  -0.00032 * Sin(3 * l[3] - 7 * l[4] + 2 * pi[3] + 2 * pi[4]) +
                  0.00030 * Sin(4 * (l[3] - l[4])) +
                  0.00029 * Sin(l[3] + pi[4] - 2 * Pi - 2 * G) +
                  -0.00028 * Sin(w[3] + psi - 2 * Pi - 2 * G) +
                  0.00026 * Sin(l[3] - Pi - G) +
                  0.00024 * Sin(l[2] - 3 * l[3] + 2 * l[4]) +
                  0.00021 * Sin(l[3] - Pi - G) +
                  -0.00021 * Sin(l[3] - pi[2]) +
                  0.00017 * Sin(2 * (l[3] - pi[3]));
            SIGMA[4] =
                0.84287 * Sin(l[4] - pi[4]) +
                  0.03431 * Sin(pi[4] - pi[3]) +
                  -0.03305 * Sin(2 * (psi - Pi)) +
                  -0.03211 * Sin(G) +
                  -0.01862 * Sin(l[4] - pi[3]) +
                  0.01186 * Sin(psi - w[4]) +
                  0.00623 * Sin(l[4] + pi[4] - 2 * G - 2 * Pi) +
                  0.00387 * Sin(2 * (l[4] - pi[4])) +
                  -0.00284 * Sin(5 * G_ - 2 * G + ToRadians(52.225)) +
                  -0.00234 * Sin(2 * (psi - pi[4])) +
                  -0.00223 * Sin(l[3] - l[4]) +
                  -0.00208 * Sin(l[4] - Pi) +
                  0.00178 * Sin(psi + w[4] - 2 * pi[4]) +
                  0.00134 * Sin(pi[4] - Pi) +
                  0.00125 * Sin(2 * (l[4] - G - Pi)) +
                  -0.00117 * Sin(2 * G) +
                  -0.00112 * Sin(2 * (l[3] - l[4])) +
                  0.00107 * Sin(3 * l[3] - 7 * l[4] + 4 * pi[4]) +
                  0.00102 * Sin(l[4] - G - Pi) +
                  0.00096 * Sin(2 * l[4] - psi - w[4]) +
                  0.00087 * Sin(2 * (psi - w[4])) +
                  -0.00085 * Sin(3 * l[3] - 7 * l[4] + pi[3] + 3 * pi[4]) +
                  0.00085 * Sin(l[3] - 2 * l[4] + pi[4]) +
                  -0.00081 * Sin(2 * (l[4] - psi)) +
                  0.00071 * Sin(l[4] + pi[4] - 2 * Pi - 3 * G) +
                  0.00061 * Sin(l[1] - l[4]) +
                  -0.00056 * Sin(psi - w[3]) +
                  -0.00054 * Sin(l[3] - 2 * l[4] + pi[3]) +
                  0.00051 * Sin(l[2] - l[4]) +
                  0.00042 * Sin(2 * (psi - G - Pi)) +
                  0.00039 * Sin(2 * (pi[4] - w[4])) +
                  0.00036 * Sin(psi + Pi - pi[4] - w[4]) +
                  0.00035 * Sin(2 * G_ - G + ToRadians(188.37)) +
                  -0.00035 * Sin(l[4] - pi[4] + 2 * Pi - 2 * psi) +
                  -0.00032 * Sin(l[4] + pi[4] - 2 * Pi - G) +
                  0.00030 * Sin(2 * G_ - 2 * G + ToRadians(149.15)) +
                  0.00029 * Sin(3 * l[3] - 7 * l[4] + 2 * pi[3] + 2 * pi[4]) +
                  0.00028 * Sin(l[4] - pi[4] + 2 * psi - 2 * Pi) +
                  -0.00028 * Sin(2 * (l[4] - w[4])) +
                  -0.00027 * Sin(pi[3] - pi[4] + w[3] - w[4]) +
                  -0.00026 * Sin(5 * G_ - 3 * G + ToRadians(188.37)) +
                  0.00025 * Sin(w[4] - w[3]) +
                  -0.00025 * Sin(l[2] - 3 * l[3] + 2 * l[4]) +
                  -0.00023 * Sin(3 * (l[3] - l[4])) +
                  0.00021 * Sin(2 * l[4] - 2 * Pi - 3 * G) +
                  -0.00021 * Sin(2 * l[3] - 3 * l[4] + pi[4]) +
                  0.00019 * Sin(l[4] - pi[4] - G) +
                  -0.00019 * Sin(2 * l[4] - pi[3] - pi[4]) +
                  -0.00018 * Sin(l[4] - pi[4] + G) +
                  -0.00016 * Sin(l[4] + pi[3] - 2 * Pi - 2 * G);

            // True longitudes of the sattelites:
            double[] L = new double[5];
            for (int i = 0; i < 5; i++)
            {
                L[i] = ToRadians(To360(l_deg[i] + SIGMA[i]));
                SIGMA[i] = ToRadians(SIGMA[i]);
            }

            double[] BB = new double[5];

            BB[1] = Atan(
                0.0006393 * Sin(L[1] - w[1]) +
                0.0001825 * Sin(L[1] - w[2]) +
                0.0000329 * Sin(L[1] - w[3]) +
                -0.0000311 * Sin(L[1] - psi) +
                0.0000093 * Sin(L[1] - w[4]) +
                0.0000075 * Sin(3 * L[1] - 4 * l[2] - 1.9927 * SIGMA[1] + w[2]) +
                0.0000046 * Sin(L[1] + psi - 2 * Pi - 2 * G));

            BB[2] = Atan(
                0.0081004 * Sin(L[2] - w[2]) +
                0.0004512 * Sin(L[2] - w[3]) +
                -0.0003284 * Sin(L[2] - psi) +
                0.0001160 * Sin(L[2] - w[4]) +
                0.0000272 * Sin(l[1] - 2 * l[3] + 1.0146 * SIGMA[2] + w[2]) +
                -0.0000144 * Sin(L[2] - w[1]) +
                0.0000143 * Sin(L[2] + psi - 2 * Pi - 2 * G) +
                0.0000035 * Sin(L[2] - psi + G) +
                -0.0000028 * Sin(l[1] - 2 * l[3] + 1.0146 * SIGMA[2] + w[3]));
            BB[3] = Atan(
                0.0032402 * Sin(L[3] - w[3]) +
                -0.0016911 * Sin(L[3] - psi) +
                0.0006847 * Sin(L[3] - w[4]) +
                -0.0002797 * Sin(L[3] - w[2]) +
                0.0000321 * Sin(L[3] + psi - 2 * Pi - 2 * G) +
                0.0000051 * Sin(L[3] - psi + G) +
                -0.0000045 * Sin(L[3] - psi - G) +
                -0.0000045 * Sin(L[3] + psi - 2 * Pi) +
                0.0000037 * Sin(L[3] + psi - 2 * Pi - 3 * G) +
                0.0000030 * Sin(2 * l[2] - 3 * L[3] + 4.03 * SIGMA[3] + w[2]) +
                -0.0000021 * Sin(2 * l[2] - 3 * L[3] + 4.03 * SIGMA[3] + w[3]));

            BB[4] = Atan(
                -0.0076579 * Sin(L[4] - psi) +
                0.0044134 * Sin(L[4] - w[4]) +
                -0.0005112 * Sin(L[4] - w[3]) +
                0.0000773 * Sin(L[4] + psi - 2 * Pi - 2 * G) +
                0.0000104 * Sin(L[4] - psi + G) +
                -0.0000102 * Sin(L[4] - psi - G) +
                0.0000088 * Sin(L[4] + psi - 2 * Pi - 3 * G) +
                -0.0000038 * Sin(L[4] + psi - 2 * Pi - G));

            double[] R = new double[5];
            R[1] =
                5.90569 * (1 + (-0.0041339 * Cos(2 * (l[1] - l[2])) +
                -0.0000387 * Cos(l[1] - pi[3]) +
                -0.0000214 * Cos(l[1] - pi[4]) +
                0.0000170 * Cos(l[1] - l[2]) +
                -0.0000131 * Cos(4 * (l[1] - l[2])) +
                0.0000106 * Cos(l[1] - l[3]) +
                -0.0000066 * Cos(l[1] + pi[3] - 2 * Pi - 2 * G)));
            R[2] =
                9.39657 * (1 + (0.0093848 * Cos(l[1] - l[2]) +
                -0.0003116 * Cos(l[2] - pi[3]) +
                -0.0001744 * Cos(l[2] - pi[4]) +
                -0.0001442 * Cos(l[2] - pi[2]) +
                0.0000553 * Cos(l[2] - l[3]) +
                0.0000523 * Cos(l[1] - l[3]) +
                -0.0000290 * Cos(2 * (l[1] - l[2])) +
                0.0000164 * Cos(2 * (l[2] - w[2])) +
                0.0000107 * Cos(l[1] - 2 * l[3] + pi[3]) +
                -0.0000102 * Cos(l[2] - pi[1]) +
                -0.0000091 * Cos(2 * (l[1] - l[3]))));
            R[3] =
                14.98832 * (1 + (-0.0014388 * Cos(l[3] - pi[3]) +
                -0.0007919 * Cos(l[3] - pi[4]) +
                0.0006342 * Cos(l[2] - l[3]) +
                -0.0001761 * Cos(2 * (l[3] - l[4])) +
                0.0000294 * Cos(l[3] - l[4]) +
                -0.0000156 * Cos(3 * (l[3] - l[4])) +
                0.0000156 * Cos(l[1] - l[3]) +
                -0.0000153 * Cos(l[1] - l[2]) +
                0.0000070 * Cos(2 * l[2] - 3 * l[3] + pi[3]) +
                -0.0000051 * Cos(l[3] + pi[3] - 2 * Pi - 2 * G)));
            R[4] =
                26.36273 * (1 + (-0.0073546 * Cos(l[4] - pi[4]) +
                0.0001621 * Cos(l[4] - pi[3]) +
                0.0000974 * Cos(l[3] - l[4]) +
                -0.0000543 * Cos(l[4] + pi[4] - 2 * Pi - 2 * G) +
                -0.0000271 * Cos(2 * (l[4] - pi[4])) +
                0.0000182 * Cos(l[4] - Pi) +
                0.0000177 * Cos(2 * (l[3] - l[4])) +
                -0.0000167 * Cos(2 * l[4] - psi - w[4]) +
                0.0000167 * Cos(psi - w[4]) +
                -0.0000155 * Cos(2 * (l[4] - Pi - G)) +
                0.0000142 * Cos(2 * (l[4] - psi)) +
                0.0000105 * Cos(l[1] - l[4]) +
                0.0000092 * Cos(l[2] - l[4]) +
                -0.0000089 * Cos(l[4] - Pi - G) +
                -0.0000062 * Cos(l[4] + pi[4] - 2 * Pi - 3 * G) +
                0.0000048 * Cos(2 * (l[4] - w[4]))));

            double T0 = (jd - 2433282.423) / 36525.0;
            double P = ToRadians(1.3966626 * T0 + 0.0003088 * T0 * T0);

            for (int i = 0; i < 5; i++)
            {
                L[i] += P;
            }
            psi += P;

            double T = (jd - 2415020.5) / 36525;
            double I = ToRadians(3.120262 + 0.0006 * T);

            double[] X = new double[6];
            double[] Y = new double[6];
            double[] Z = new double[6];

            for (int i = 1; i < 5; i++)
            {
                X[i] = R[i] * Cos(L[i] - psi) * Cos(BB[i]);
                Y[i] = R[i] * Sin(L[i] - psi) * Cos(BB[i]);
                Z[i] = R[i] * Sin(BB[i]);
            }

            X[5] = 0; Y[5] = 0; Z[5] = 1;

            double[] A1 = new double[6];
            double[] B1 = new double[6];
            double[] C1 = new double[6];

            for (int i = 1; i < 6; i++)
            {
                A1[i] = X[i];
                B1[i] = Y[i] * Cos(I) - Z[i] * Sin(I);
                C1[i] = Y[i] * Sin(I) + Z[i] * Cos(I);
            }

            double[] A2 = new double[6];
            double[] B2 = new double[6];
            double[] C2 = new double[6];

            double T1 = (jd - 2451545.0) / 36525;
            double T2 = T1 * T1;
            double T3 = T2 * T1;

            double OMEGA = 100.464407 + 1.0209774 * T1 + 0.00040315 * T2 + 0.000000404 * T3;
            OMEGA = ToRadians(OMEGA);

            double Inc = 1.303267 - 0.0054965 * T1 + 0.00000466 * T2 + 0.000000002 * T3;
            Inc = ToRadians(Inc);

            double PHI = psi - OMEGA;

            for (int i = 5; i >= 1; i--)
            {
                A2[i] = A1[i] * Cos(PHI) - B1[i] * Sin(PHI);
                B2[i] = A1[i] * Sin(PHI) + B1[i] * Cos(PHI);
                C2[i] = C1[i];
            }

            double[] A3 = new double[6];
            double[] B3 = new double[6];
            double[] C3 = new double[6];

            for (int i = 5; i >= 1; i--)
            {
                A3[i] = A2[i];
                B3[i] = B2[i] * Cos(Inc) - C2[i] * Sin(Inc);
                C3[i] = B2[i] * Sin(Inc) + C2[i] * Cos(Inc);
            }

            double[] A4 = new double[6];
            double[] B4 = new double[6];
            double[] C4 = new double[6];

            for (int i = 5; i >= 1; i--)
            {
                A4[i] = A3[i] * Cos(OMEGA) - B3[i] * Sin(OMEGA);
                B4[i] = A3[i] * Sin(OMEGA) + B3[i] * Cos(OMEGA);
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
                double x = jupiter.R * Cos(ToRadians(jupiter.B)) * Cos(ToRadians(jupiter.L)) + Radius * Cos(ToRadians(earth.L + 180));
                double y = jupiter.R * Cos(ToRadians(jupiter.B)) * Sin(ToRadians(jupiter.L)) + Radius * Sin(ToRadians(earth.L + 180));
                double z = jupiter.R * Sin(ToRadians(jupiter.B)) + Radius * Sin(ToRadians(-earth.B));

                double Delta = Sqrt(x * x + y * y + z * z);
                double LAMBDA = Atan2(y, x);
                double alpha = Atan(z / Sqrt(x * x + y * y));

                for (int i = 5; i >= 1; i--)
                {
                    A5[i] = A4[i] * Sin(LAMBDA) - B4[i] * Cos(LAMBDA);
                    B5[i] = A4[i] * Cos(LAMBDA) + B4[i] * Sin(LAMBDA);
                    C5[i] = C4[i];
                }

                double[] A6 = new double[6];
                double[] B6 = new double[6];
                double[] C6 = new double[6];

                for (int i = 5; i >= 1; i--)
                {
                    A6[i] = A5[i];
                    B6[i] = C5[i] * Sin(alpha) + B5[i] * Cos(alpha);
                    C6[i] = C5[i] * Cos(alpha) - B5[i] * Sin(alpha);
                }

                double D = Atan2(A6[5], C6[5]);

                CrdsRectangular[] rectangular = new CrdsRectangular[4];

                for (int i = 0; i < 4; i++)
                {
                    rectangular[i] = new CrdsRectangular(
                        A6[i + 1] * Cos(D) - C6[i + 1] * Sin(D),
                        A6[i + 1] * Sin(D) + C6[i + 1] * Cos(D),
                        B6[i + 1]
                    );
                }

                double[] K = { 17295, 21819, 27558, 36548 };

                for (int i = 0; i < 4; i++)
                {
                    rectangular[i].X += Abs(rectangular[i].Z) / K[i] * Sqrt(1 - Pow(rectangular[i].X / R[i + 1], 2));
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
