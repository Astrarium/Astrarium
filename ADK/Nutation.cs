using System;
using System.Collections.Generic;
using System.Text;

namespace ADK
{
    /// <summary>
    /// Contains methods for calculation of mean and true obliquity of the ecliptic,
    /// and also methods for calculation of nutation effects.
    /// </summary>
    public static class Nutation
    {
        /// <summary>
        /// Calculates the mean obliquity of the ecliptic (ε0).
        /// </summary>
        /// <param name="jd">Julian Day, corresponding to the given date.</param>
        /// <returns>Returns mean obliquity of the ecliptic for the given date, expressed in degrees.</returns>
        /// <remarks>
        /// AA(II) formula 22.3.
        /// </remarks>
        public static double MeanObliquity(double jd)
        {
            double T = (jd - 2451545) / 36525.0;

            double[] U = new double[11];
            double[] c = new double[] { 84381.448, -4680.93, -1.55, +1999.25, -51.38, -249.67, -39.05, +7.12, +27.87, +5.79, +2.45 };

            U[0] = 1;
            U[1] = T / 100.0;
            for (int i = 2; i <= 10; i++)
            {
                U[i] = U[i - 1] * U[1];
            }

            double epsilon0 = 0;
            for (int i = 0; i <= 10; i++)
            {
                epsilon0 += c[i] * U[i];
            }
            
            return epsilon0 / 3600.0;
        }

        /// <summary>
        /// Calculates the true obliquity of the ecliptic (ε).
        /// </summary>
        /// <param name="jd">Julian Day, corresponding to the given date.</param>
        /// <returns>Returns true obliquity of the ecliptic for the given date, expressed in degrees.</returns>
        /// <remarks>
        /// AA(II) chapter 22.
        /// </remarks>
        public static double TrueObliquity(double jd)
        {
            return MeanObliquity(jd) + NutationInObliquity(jd);
        }

        /// <summary>
        /// Calculates the nutation in obliquity (Δε) for given date.
        /// </summary>
        /// <param name="jd">Julian Day, corresponding to the given date.</param>
        /// <returns>Returns nutation in obliquity value in degrees.</returns>
        public static double NutationInObliquity(double jd)
        {
            double T = (jd - 2451545) / 36525.0;

            // Longitude of the ascending node of Moon's mean orbit on the ecliptic, 
            // measured from the mean equinox of the date: 
            double Omega = 125.04452 - 1934.136261 * T;

            // Mean longutude of Sun
            double L = 280.4665 + 36000.7698 * T;

            // Mean longitude of Moon
            double L_ = 218.3165 + 481267.8813 * T;

            double deltaEpsilon = 9.20 * Math.Cos(AstroUtils.ToRadian(Omega)) + 0.57 * Math.Cos(AstroUtils.ToRadian(2 * L)) + 0.10 * Math.Cos(AstroUtils.ToRadian(2 * L_)) - 0.09 * Math.Cos(AstroUtils.ToRadian(2 * Omega));

            return deltaEpsilon / 3600.0;
        }

        /// <summary>
        /// Calculates the nutation in longitude (Δψ) for given date.
        /// </summary>
        /// <param name="jd">Julian Day, corresponding to the given date.</param>
        /// <returns>Returns nutation in longitude value in degrees.</returns>
        public static double NutationInLongitude(double jd)
        {
            double T = (jd - 2451545) / 36525.0;

            // Longitude of the ascending node of Moon's mean orbit on the ecliptic, 
            // measured from the mean equinox of the date: 
            double Omega = 125.04452 - 1934.136261 * T;

            // Mean longutude of Sun
            double L = 280.4665 + 36000.7698 * T;

            // Mean longitude of Moon
            double L_ = 218.3165 + 481267.8813 * T;

            double deltaPsi = -17.20 * Math.Sin(AstroUtils.ToRadian(Omega)) - 1.32 * Math.Sin(AstroUtils.ToRadian(2 * L)) - 0.23 * Math.Sin(AstroUtils.ToRadian(2 * L_)) + 0.21 * Math.Sin(AstroUtils.ToRadian(2 * Omega));

            return deltaPsi / 3600.0;
        }
    }
}
