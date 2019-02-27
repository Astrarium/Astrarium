using System;

namespace ADK
{
    /// <summary>
    /// Provides methods for calculation of ephemerides of the Sun.
    /// </summary>
    public static class SolarEphem
    {
        /// <summary>
        /// Calculates parallax of the Sun
        /// </summary>
        /// <param name="distance">Distance to the Sun, in A.U.</param>
        /// <returns>Parallax value in degrees.</returns>
        /// <remarks>
        /// Method is taken from AA(II), p. 279.
        /// </remarks>
        public static double Parallax(double distance)
        {
            return 8.794 / distance / 3600;
        }

        /// <summary>
        /// Gets solar semidiameter
        /// </summary>
        /// <param name="distance">Distance to the Sun, in AU.</param>
        /// <returns>
        /// Solar semidiameter, in arcseconds.
        /// </returns>
        /// <remarks>
        /// Formula is taken from AA(I) book, p. 359.
        /// </remarks>
        public static double Semidiameter(double distance)
        {
            return 959.63 / distance;
        }

        /// <summary>
        /// Gets Carrington rotation number of Sun.
        /// </summary>
        /// <param name="jd">Julian day to calculate the Carrington rotation number</param>
        /// <returns>Carrington rotation number for the given instant.</returns>
        /// <remarks>
        /// The formula is taken from Duffeth-Smith book, page 75.
        /// </remarks>
        public static long CarringtonNumber(double jd)
        {
            return Convert.ToInt64(1690 + (jd - 2444235.34) / 27.2753);
        }

        /// <summary>
        /// Calculates instant of beginning of astronomical season for given julian day.
        /// </summary>
        /// <param name="jd">Given year for which the date of beginning of season will be calculated.</param>
        /// <param name="season">Astronomical season.</param>
        /// <returns>Returns local date and time of beginning of the season.</returns>
        /// <remarks>
        /// Method is taken from AA(I), chapter 26.
        /// </remarks>
        public static double Season(double jd, Season season)
        {
            int year = new Date(jd).Year;
            
            double jd0 = 0;
            if (year >= -1000 && year < 1000)
            {
                double Y = year / 1000.0;
                double Y2 = Y * Y;
                double Y3 = Y2 * Y;
                double Y4 = Y3 * Y;

                switch (season)
                {
                    case ADK.Season.Spring:
                        jd0 = 1721139.29189 + 365242.13740 * Y + 0.06134 * Y2 + 0.00111 * Y3 - 0.00071 * Y4;
                        break;
                    case ADK.Season.Summer:
                        jd0 = 1721233.25401 + 365241.72562 * Y - 0.05323 * Y2 + 0.00907 * Y3 + 0.00025 * Y4;
                        break;
                    case ADK.Season.Autumn:
                        jd0 = 1721325.70455 + 365242.49558 * Y - 0.11677 * Y2 - 0.00297 * Y3 + 0.00074 * Y4;
                        break;
                    case ADK.Season.Winter:
                        jd0 = 1721414.39987 + 365242.88257 * Y - 0.00769 * Y2 - 0.00933 * Y3 - 0.00006 * Y4;
                        break;
                    default:
                        break;
                }
            }
            if (year >= 1000 && year <= 3000)
            {
                double Y = (year - 2000) / 1000.0;
                double Y2 = Y * Y;
                double Y3 = Y2 * Y;
                double Y4 = Y3 * Y;

                switch (season)
                {
                    case ADK.Season.Spring:
                        jd0 = 2451623.80984 + 365242.37404 * Y + 0.05169 * Y2 - 0.00411 * Y3 - 0.00057 * Y4;
                        break;
                    case ADK.Season.Summer:
                        jd0 = 2451716.56767 + 365241.62603 * Y + 0.00325 * Y2 + 0.00888 * Y3 - 0.00030 * Y4;
                        break;
                    case ADK.Season.Autumn:
                        jd0 = 2451810.21715 + 365242.01767 * Y - 0.11575 * Y2 + 0.00337 * Y3 + 0.00078 * Y4;
                        break;
                    case ADK.Season.Winter:
                        jd0 = 2451900.05952 + 365242.74049 * Y - 0.06223 * Y2 - 0.00823 * Y3 + 0.00032 * Y4;
                        break;
                    default:
                        break;
                }
            }

            double T = (jd0 - 2451545.0) / 36525.0;
            double W = 35999.373 * T - 2.47;
            double delta_lambda = 1 + 0.0334 * Math.Cos(Angle.ToRadians(W)) + 0.0007 * Math.Cos(Angle.ToRadians(2 * W));

            double S =
                485 * Math.Cos(Angle.ToRadians(324.96 + 1934.136 * T)) +
                203 * Math.Cos(Angle.ToRadians(337.23 + 32964.467 * T)) +
                199 * Math.Cos(Angle.ToRadians(342.08 + 20.186 * T)) +
                182 * Math.Cos(Angle.ToRadians(27.85 + 445267.112 * T)) +
                156 * Math.Cos(Angle.ToRadians(73.14 + 45036.886 * T)) +
                136 * Math.Cos(Angle.ToRadians(171.52 + 22518.443 * T)) +
                77 * Math.Cos(Angle.ToRadians(222.54 + 65928.934 * T)) +
                74 * Math.Cos(Angle.ToRadians(296.72 + 3034.906 * T)) +
                70 * Math.Cos(Angle.ToRadians(243.58 + 9037.513 * T)) +
                58 * Math.Cos(Angle.ToRadians(119.81 + 33718.147 * T)) +
                52 * Math.Cos(Angle.ToRadians(297.17 + 150.678 * T)) +
                50 * Math.Cos(Angle.ToRadians(21.02 + 2281.226 * T)) +
                45 * Math.Cos(Angle.ToRadians(247.54 + 29929.562 * T)) +
                44 * Math.Cos(Angle.ToRadians(325.15 + 31555.956 * T)) +
                29 * Math.Cos(Angle.ToRadians(60.93 + 4443.417 * T)) +
                18 * Math.Cos(Angle.ToRadians(155.12 + 67555.328 * T)) +
                17 * Math.Cos(Angle.ToRadians(288.79 + 4562.452 * T)) +
                16 * Math.Cos(Angle.ToRadians(198.04 + 62894.029 * T)) +
                14 * Math.Cos(Angle.ToRadians(199.76 + 31436.921 * T)) +
                12 * Math.Cos(Angle.ToRadians(95.39 + 14577.848 * T)) +
                12 * Math.Cos(Angle.ToRadians(287.11 + 31931.756 * T)) +
                12 * Math.Cos(Angle.ToRadians(320.81 + 34777.259 * T)) +
                9 * Math.Cos(Angle.ToRadians(227.73 + 1222.114 * T)) +
                8 * Math.Cos(Angle.ToRadians(15.45 + 16859.074 * T));

            return jd0 + (0.00001 * S) / delta_lambda;
        }
    }
}
