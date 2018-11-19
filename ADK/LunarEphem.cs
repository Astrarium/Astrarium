using System;

namespace ADK
{
    /// <summary>
    /// Provides methods for calculation of ephemerides of the Moon
    /// </summary>
    public static class LunarEphem
    {
        /// <summary>
        /// Calculates Moon horizontal equatorial parallax. 
        /// </summary>
        /// <param name="distance">Distance between Moon and Earth centers, in kilometers.</param>
        /// <returns>Returns value of parallax in degrees.</returns>
        /// <remarks>Taken from AA(II), page 390</remarks>
        // TODO: test
        public static double Parallax(double distance)
        {
            return Angle.ToDegrees(Math.Asin(6378.14 / distance));
        }

        /// <summary>
        /// Calculates visible semidiameter of the Moon, in seconds of arc.
        /// </summary>
        /// <param name="distance">Distance to the Moon, in kilometers</param>
        /// <returns>Visible semidiameter of the Moon, in seconds of arc.</returns>
        /// <remarks>Taken from AA(II), page 391</remarks>
        // TODO: test
        public static double Semidiameter(double distance)
        {
            return 358473400.0 / distance;
        }

        /// <summary>
        /// Calculates position angle of Moon bright limb.
        /// </summary>
        /// <param name="sun">Geocentric equatorial coordinates of the Sun</param>
        /// <param name="moon">Geocentric equatorial coordinates of the Moon</param>
        /// <returns>Position angle of Moon bright limb, in degrees</returns>
        /// <remarks>
        /// Method is taken from AA(II), formula 48.5.
        /// </remarks>
        public static double PositionAngleOfBrightLimb(CrdsEquatorial sun, CrdsEquatorial moon)
        {
            double sunDelta = Angle.ToRadians(sun.Delta);
            double moonDelta = Angle.ToRadians(moon.Delta);
            double deltaAlpha = Angle.ToRadians(sun.Alpha - moon.Alpha);

            double y = Math.Cos(sunDelta) * Math.Sin(deltaAlpha);
            double x = Math.Sin(sunDelta) * Math.Cos(moonDelta) - Math.Cos(sunDelta) * Math.Sin(moonDelta) * Math.Cos(deltaAlpha);

            return Angle.To360(Angle.ToDegrees(Math.Atan2(y, x)));
        }

        // TODO: reference to PAWC book, tests
        public static double PositionAngleOfNorthCusp(double PAlimb)
        {
            if (PAlimb < 180)
            {
                return Angle.To360(PAlimb - 90);
            }
            else
            {
                return Angle.To360(PAlimb + 90);
            }
        }

        // TODO: not finished yet: AA(II), p.373 
        public static double PositionAngleOfAxis(double jd)
        {
            double T = (jd - 2451545.0) / 36525.0;

            double T2 = T * T;
            double T3 = T2 * T;
            double T4 = T3 * T;

            double Omega = 125.0445479 - 1934.1362891 * T + 0.0020754 * T2 + T3 / 467441.0 - T4 / 60616000.0;

            // Moon's argument of latitude (mean dinstance of the Moon from its ascending node)
            double F = 93.2720950 + 483202.0175233 * T - 0.0036539 * T2 - T3 / 3526000.0 + T4 / 863310000.0;

            return 0;
        }

        /// <summary>
        /// Gets geocentric elongation angle of the Moon from Sun
        /// </summary>
        /// <param name="sun">Ecliptical geocentrical coordinates of the Sun</param>
        /// <param name="moon">Ecliptical geocentrical coordinates of the Moon</param>
        /// <returns>Geocentric elongation angle, in degrees, from -180 to 180.
        /// Negative sign means western elongation, positive eastern.
        /// </returns>
        /// <remarks>
        /// AA(II), formula 48.2
        /// </remarks>
        public static double Elongation(CrdsEcliptical sun, CrdsEcliptical moon)
        {
            double beta = Angle.ToRadians(moon.Beta);
            double lambda = Angle.ToRadians(moon.Lambda);
            double lambda0 = Angle.ToRadians(sun.Lambda);

            double s = sun.Lambda;
            double b = moon.Lambda;

            if (Math.Abs(s - b) > 180)
            {
                if (s < b)
                {
                    s += 360;
                }
                else
                {
                    b += 360;
                }
            }

            return Math.Sign(b - s) * Angle.ToDegrees(Math.Acos(Math.Cos(beta) * Math.Cos(lambda - lambda0)));
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
        /// Gets magnitude of the Moon by its phase angle.
        /// </summary>
        /// <param name="phaseAngle">Phase angle value, in degrees, from 0 to 180.</param>
        /// <returns>Moon magnitude</returns>
        /// <remarks>
        /// Formula is taken from <see href="https://astronomy.stackexchange.com/questions/10246/is-there-a-simple-analytical-formula-for-the-lunar-phase-brightness-curve"/>
        /// </remarks>
        public static double Magnitude(double phaseAngle)
        {
            double psi = Angle.ToRadians(phaseAngle);
            double psi4 = Math.Pow(psi, 4);

            return -12.73 + 1.49 * Math.Abs(psi) + 0.043 * psi4;
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
