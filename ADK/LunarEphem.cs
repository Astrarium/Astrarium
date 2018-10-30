using System;

namespace ADK
{
    /// <summary>
    /// Provides methods for calculation of ephemerides of the Moon
    /// </summary>
    public static class LunarEphem
    {
        /// <summary>
        /// Gets geocentric elongation angle of the Moon from Sun
        /// </summary>
        /// <param name="sun">Ecliptical geocentrical coordinates of the Sun</param>
        /// <param name="moon">Ecliptical geocentrical coordinates of the Moon</param>
        /// <returns>Geocentric elongation angle, in degrees, from 0 to 180.</returns>
        /// <remarks>
        /// AA(II), formula 48.2
        /// </remarks>
        public static double Elongation(CrdsEcliptical sun, CrdsEcliptical moon)
        {
            double beta = Angle.ToRadians(moon.Beta);
            double lambda = Angle.ToRadians(moon.Lambda);
            double lambda0 = Angle.ToRadians(sun.Lambda);

            return Angle.ToDegrees(Math.Acos(Math.Cos(beta) * Math.Cos(lambda - lambda0)));
        }

        /// <summary>
        /// Calculates phase angle of the Moon
        /// </summary>
        /// <param name="psi">Geocentric elongation of the Moon.</param>
        /// <param name="R">Distance Earth-Sun, in kilometers</param>
        /// <param name="Delta">Distance Earth-Moon, in kilometers</param>
        /// <returns>Phase angle, in degrees, from 0 to 180</returns>
        /// <remarks>
        /// AA(II), formula 48.3.
        /// </remarks>
        public static double PhaseAngle(double psi, double R, double Delta)
        {
            psi = Angle.ToRadians(Math.Abs(psi));
            double phaseAngle = Angle.ToDegrees(Math.Atan(R * Math.Sin(psi) / (Delta - R * Math.Cos(psi))));
            if (phaseAngle < 0) phaseAngle += 180;
            return phaseAngle;
        }

        /// <summary>
        /// Gets phase value (illuminated fraction of the Moon disk).
        /// </summary>
        /// <param name="phaseAngle">Phase angle of the Moon, in degrees.</param>
        /// <returns>Illuminated fraction of the Moon disk, from 0 to 1.</returns>
        /// <remarks>
        /// AA(II), formula 48.1
        /// </remarks>
        public static double Phase(double phaseAngle)
        {
            return (1 + Math.Cos(Angle.ToRadians(phaseAngle))) / 2;
        }

        /// <summary>
        /// Gets longitude of mean ascending node of Lunar orbit for given instant.
        /// </summary>
        /// <param name="jd">Julian Day</param>
        /// <returns>Longitude of mean ascending node of Lunar orbit, in degrees.</returns>
        public static double MeanAscendingNode(double jd)
        {
            return AscendingNode(jd, trueAscendingNode: false);
        }

        /// <summary>
        /// Gets longitude of true ascending node of Lunar orbit for given instant.
        /// </summary>
        /// <param name="jd">Julian Day</param>
        /// <returns>Longitude of true ascending node of Lunar orbit, in degrees.</returns>
        public static double TrueAscendingNode(double jd)
        {
            return AscendingNode(jd, trueAscendingNode: true);
        }

        /// <summary>
        /// Gets longitude of ascending node of Lunar orbit for given instant.
        /// </summary>
        /// <param name="jd">Julian Day</param>
        /// <param name="trueAscendingNode">True if position of true ascending node is needed, false for mean position</param>
        /// <returns>Longitude of ascending node of Lunar orbit, in degrees.</returns>
        private static double AscendingNode(double jd, bool trueAscendingNode)
        {
            double T = (jd - 2451545.0) / 36525.0;

            double T2 = T * T;
            double T3 = T2 * T;
            double T4 = T3 * T;

            double Omega = 125.0445479 - 1934.1362891 * T + 0.0020754 * T2 + T3 / 467441.0 - T4 / 60616000.0;

            if (trueAscendingNode)
            {
                // Mean elongation of the Moon
                double D = 297.8501921 + 445267.1114034 * T - 0.0018819 * T2 + T3 / 545868.0 - T4 / 113065000.0;

                // Sun's mean anomaly
                double M = 357.5291092 + 35999.0502909 * T - 0.0001536 * T2 + T3 / 24490000.0;

                // Moon's mean anomaly
                double M_ = 134.9633964 + 477198.8675055 * T + 0.0087414 * T2 + T3 / 69699.0 - T4 / 14712000.0;

                // Moon's argument of latitude (mean dinstance of the Moon from its ascending node)
                double F = 93.2720950 + 483202.0175233 * T - 0.0036539 * T2 - T3 / 3526000.0 + T4 / 863310000.0;

                Omega +=
                    -1.4979 * Math.Sin(Angle.ToRadians(2 * (D - F)))
                    - 0.1500 * Math.Sin(Angle.ToRadians(M))
                    - 0.1226 * Math.Sin(Angle.ToRadians(2 * D))
                    + 0.1176 * Math.Sin(Angle.ToRadians(2 * F))
                    - 0.0801 * Math.Sin(Angle.ToRadians(2 * (M_ - F)));
            }

            return Angle.To360(Omega);
        }
    }
}
