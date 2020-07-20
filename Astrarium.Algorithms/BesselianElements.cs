using static Astrarium.Algorithms.Angle;
using static System.Math;

namespace Astrarium.Algorithms
{
    /// <summary>
    /// Represents set of Besselian elements describing solar eclipse appearance
    /// </summary>
    public class BesselianElements
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double D { get; set; }
        public double L1 { get; set; }
        public double L2 { get; set; }
        public double Mu { get; set; }

        /// <summary>
        /// Calculates Besselian elements for solar eclipse
        /// </summary>
        /// <param name="jd">Julian day of interest.</param>
        /// <param name="sun">Geocentrical equatorial coordinates of the Sun at the moment</param>
        /// <param name="moon">Geocentrical equatorial coordinates of the Moon at the moment</param>
        /// <param name="rs">Distance Earth-Sun center, in units of Earth equatorial radii.</param>
        /// <param name="rm">Distance Earth-Moon center, in units of Earth equatorial radii.</param>
        /// <returns>
        /// Besselian elements for solar eclipse
        /// </returns>
        /// <remarks>
        /// The method is based on formulae given here:
        /// https://de.wikipedia.org/wiki/Besselsche_Elemente
        /// </remarks>
        public static BesselianElements Calculate(double jd, CrdsEquatorial sun, CrdsEquatorial moon, double rs, double rm)
        {             
            // Sidereal time at Greenwich
            double theta = Date.MeanSiderealTime(jd);

            double aSun = ToRadians(sun.Alpha);
            double dSun = ToRadians(sun.Delta);

            double aMoon = ToRadians(moon.Alpha);
            double dMoon = ToRadians(moon.Delta);

            // Rs vector
            double[] Rs = new double[3];
            Rs[0] = rs * Cos(aSun) * Cos(dSun);
            Rs[1] = rs * Sin(aSun) * Cos(dSun);
            Rs[2] = rs * Sin(dSun);

            // Rm vector
            double[] Rm = new double[3];
            Rm[0] = rm * Cos(aMoon) * Cos(dMoon);
            Rm[1] = rm * Sin(aMoon) * Cos(dMoon);
            Rm[2] = rm * Sin(dMoon);

            double[] Rsm = new double[3];
            for (int i = 0; i < 3; i++)
            {
                Rsm[i] = Rs[i] - Rm[i];
            }

            double lenRsm = Sqrt(Rsm[0] * Rsm[0] + Rsm[1] * Rsm[1] + Rsm[2] * Rsm[2]);

            // k vector
            double[] k = new double[3];
            for (int i = 0; i < 3; i++)
            {
                k[i] = Rsm[i] / lenRsm;
            }

            double d = Asin(k[2]);
            double a = Atan2(k[1], k[0]);

            double x = rm * Cos(dMoon) * Sin(aMoon - a);
            double y = rm * (Sin(dMoon) * Cos(d) - Cos(dMoon) * Sin(d) * Cos(aMoon - a));
            double zm = rm * (Sin(dMoon) * Sin(d) + Cos(dMoon) * Cos(d) * Cos(aMoon - a));

            // Sun and Moon radii, in Earth equatorial radii
            double rhoSun = 696340.0 / 6371.0;
            double rhoMoon = 1731.1 / 6371.0;

            double sinF1 = (rhoSun + rhoMoon) / lenRsm;
            double sinF2 = (rhoSun - rhoMoon) / lenRsm;

            double F1 = Asin(sinF1);
            double F2 = Asin(sinF2);

            double zv1 = zm + rhoMoon / sinF1;
            double zv2 = zm - rhoMoon / sinF2;

            double l1 = zv1 * Tan(F1);
            double l2 = zv2 * Tan(F2);

            return new BesselianElements()
            {
                X = x,
                Y = y,
                L1 = l1,
                L2 = l2,
                D = ToDegrees(d),
                Mu = To360(theta - ToDegrees(a))
            };
        }
    }
}
