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

        // TODO: reference to PAWC book, tests, move to separate class
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
        public static double PositionAngleOfAxis(double jd, CrdsEcliptical ecl, double epsilon, double deltaPsi)
        {
            double T = (jd - 2451545.0) / 36525.0;

            double T2 = T * T;
            double T3 = T2 * T;
            double T4 = T3 * T;

            // Mean longitude of ascending node
            double Omega = 125.0445479 - 1934.1362891 * T + 0.0020754 * T2 + T3 / 467441.0 - T4 / 60616000.0;
            
            // Mean elongation of the Moon
            double D = 297.8501921 + 445267.1114034 * T - 0.0018819 * T2 + T3 / 545868.0 - T4 / 113065000.0;
            D = Angle.ToRadians(Angle.To360(D));

            // Sun's mean anomaly
            double M = 357.5291092 + 35999.0502909 * T - 0.0001536 * T2 + T3 / 24490000.0;
            M = Angle.ToRadians(Angle.To360(M));

            // Moon's mean anomaly
            double M_ = 134.9633964 + 477198.8675055 * T + 0.0087414 * T2 + T3 / 69699.0 - T4 / 14712000.0;
            M_ = Angle.ToRadians(Angle.To360(M_));

            // Moon's argument of latitude (mean dinstance of the Moon from its ascending node)
            double F = 93.2720950 + 483202.0175233 * T - 0.0036539 * T2 - T3 / 3526000.0 + T4 / 863310000.0;

            double rho =
                - 0.02752 * Math.Cos(M_)
                - 0.02245 * Math.Sin(F)
                + 0.00684 * Math.Cos(M_ - 2 * F)
                - 0.00293 * Math.Cos(2 * F)
                - 0.00085 * Math.Cos(2 * F - 2 * D)
                - 0.00054 * Math.Cos(M_ - 2 * D)
                - 0.00020 * Math.Sin(M_ + F)
                - 0.00020 * Math.Cos(M_ + 2 * F)
                - 0.00020 * Math.Cos(M_ - F)
                + 0.00014 * Math.Cos(M_ + 2 * F - 2 * D);

            double sigma =
                -0.02816 * Math.Sin(M_)
                + 0.02244 * Math.Cos(F)
                - 0.00682 * Math.Sin(M_ - 2 * F)
                - 0.00279 * Math.Sin(2 * F)
                - 0.00083 * Math.Sin(2 * F - 2 * D)
                + 0.00069 * Math.Sin(M_ - 2 * D)
                + 0.00040 * Math.Cos(M_ + F)
                - 0.00025 * Math.Sin(2 * M_)
                - 0.00023 * Math.Sin(M_ + 2 * F)
                + 0.00020 * Math.Cos(M_ - F)
                + 0.00019 * Math.Sin(M_ - F)
                + 0.00013 * Math.Sin(M_ + 2 * F - 2 * D)
                - 0.00010 * Math.Cos(M_ - 3 * F);

            double W = Angle.ToRadians(ecl.Lambda - deltaPsi - Omega);
            double beta = Angle.ToRadians(ecl.Beta);

            // Inclination of the mean lunar equator to ecliptic
            double I = Angle.ToRadians(1.54242);

            double y = Math.Sin(W) * Math.Cos(beta) * Math.Cos(I) - Math.Sin(beta) * Math.Sin(I);
            double x = Math.Cos(W) * Math.Cos(beta);

            double A = Math.Atan2(y, x);

            double b_ = Angle.ToDegrees(Math.Asin(-Math.Sin(W) * Math.Cos(beta) * Math.Sin(I) - Math.Sin(beta) * Math.Cos(I)));
            double b__ = sigma * Math.Cos(A) - rho * Math.Sin(A);

            double b = b_ + b__;

            double V = Angle.ToRadians(Omega + deltaPsi + sigma / Math.Sin(I));

            double sinIrho = Math.Sin(I + Angle.ToRadians(rho));
            double cosIrho = Math.Sin(I + Angle.ToRadians(rho));

            epsilon = Angle.ToRadians(epsilon);

            double X = sinIrho * Math.Sin(V);
            double Y = sinIrho * Math.Cos(V) * Math.Cos(epsilon) - cosIrho * Math.Sin(epsilon);

            double omega = Math.Atan2(Y, X);

            // TODO: pass RA to function
            double alpha = 0;

            double P = Math.Asin(Math.Sqrt(X * X + Y * Y) * Math.Cos(alpha - omega) / Math.Cos(Angle.ToRadians(b)));

            return Angle.ToDegrees(P);
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
