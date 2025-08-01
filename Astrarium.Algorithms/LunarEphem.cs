using System;

namespace Astrarium.Algorithms
{
    /// <summary>
    /// Provides methods for calculation of ephemerides of the Moon.
    /// </summary>
    public static class LunarEphem
    {
        /// <summary>
        /// Period that takes the Moon to return to a similar position among the stars, in days.
        /// </summary>
        public const double SIDEREAL_PERIOD = 27.321661;

        /// <summary>
        /// Period between two same successive lunar phases, in days.
        /// </summary>
        public const double SINODIC_PERIOD = 29.530588;

        /// <summary>
        /// The time that elapses between two passages of the Moon at its perigee/apogee, in days.
        /// Librations in longitude have same periodicity too.
        /// </summary>
        public const double ANOMALISTIC_PERIOD = 27.55455;

        /// <summary>
        /// The period in which the Moon returns to the same node of its orbit, in days.
        /// Librations in latitude have same periodicity too.
        /// </summary>
        public const double DRACONIC_PERIOD = 27.2122204;

        /// <summary>
        /// Length of saros cycle, in days.
        /// </summary>
        public const double SAROS = 6585.3211;

        /// <summary>
        /// Average apparent daily motion of the Moon, among the stars, in degrees per day.
        /// </summary>
        public const double AVERAGE_DAILY_MOTION = 360 / SIDEREAL_PERIOD;

        /// <summary>
        /// Calculates Moon horizontal equatorial parallax. 
        /// </summary>
        /// <param name="distance">Distance between Moon and Earth centers, in kilometers.</param>
        /// <returns>Returns value of parallax in degrees.</returns>
        /// <remarks>Taken from AA(II), page 390</remarks>
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

        /// <summary>
        /// Calculates position angle of Moon north cusp.
        /// </summary>
        /// <param name="PAlimb">Position angle of bright limb, in degrees.</param>
        /// <returns>Position angle of Moon north cusp, in degrees</returns>
        /// <remarks>
        /// Method is based on relations described in book PAWC, chapter 55, p. 110.
        /// </remarks>
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

        /// <summary>
        /// Calculates position angle of Moon axis for given instant.
        /// </summary>
        /// <param name="jd">Julian Ephemeris Day to calculate position angle.</param>
        /// <param name="ecl">Apparent geocentrical/topocentrical ecliptical coordinates of the Moon for that instant.</param>
        /// <param name="epsilon">True obliquity of the ecliptic, in degrees.</param>
        /// <param name="deltaPsi">Nutation in longitude, in degrees.</param>
        /// <returns>
        /// Position angle of Moon axis for given instant, in degrees.
        /// </returns>
        /// <remarks>
        /// Method is taken from AA(II), p.373.
        /// </remarks>
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

            // Moon's mean anomaly
            double M_ = 134.9633964 + 477198.8675055 * T + 0.0087414 * T2 + T3 / 69699.0 - T4 / 14712000.0;
            M_ = Angle.ToRadians(Angle.To360(M_));

            // Moon's argument of latitude (mean dinstance of the Moon from its ascending node)
            double F = 93.2720950 + 483202.0175233 * T - 0.0036539 * T2 - T3 / 3526000.0 + T4 / 863310000.0;
            F = Angle.ToRadians(Angle.To360(F));

            // Inclination of the mean lunar equator to ecliptic
            double I = Angle.ToRadians(1.54242);

            double rho =
                -0.02752 * Math.Cos(M_)
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

            double W = Angle.ToRadians(Angle.To360(ecl.Lambda - deltaPsi - Omega));
            double beta = Angle.ToRadians(ecl.Beta);

            double y = Math.Sin(W) * Math.Cos(beta) * Math.Cos(I) - Math.Sin(beta) * Math.Sin(I);
            double x = Math.Cos(W) * Math.Cos(beta);

            double A = Math.Atan2(y, x);

            // optical libration in latitude, in degrees
            double b_ = Angle.ToDegrees(Math.Asin(-Math.Sin(W) * Math.Cos(beta) * Math.Sin(I) - Math.Sin(beta) * Math.Cos(I)));

            // physical libration in latitude, in degrees
            double b__ = sigma * Math.Cos(A) - rho * Math.Sin(A);

            // total libration in latitude, in degrees
            double b = b_ + b__;

            double V = Angle.ToRadians(Omega + deltaPsi + sigma / Math.Sin(I));

            double sinIrho = Math.Sin(I + Angle.ToRadians(rho));
            double cosIrho = Math.Cos(I + Angle.ToRadians(rho));

            double alpha = Angle.ToRadians(ecl.ToEquatorial(epsilon).Alpha);
            epsilon = Angle.ToRadians(epsilon);

            double X = sinIrho * Math.Sin(V);
            double Y = sinIrho * Math.Cos(V) * Math.Cos(epsilon) - cosIrho * Math.Sin(epsilon);

            double omega = Math.Atan2(X, Y);

            double P = Math.Asin(Math.Sqrt(X * X + Y * Y) * Math.Cos(alpha - omega) / Math.Cos(Angle.ToRadians(b)));

            return Angle.ToDegrees(P);
        }

        /// <summary>
        /// Calculates libration angles of the Moon for given instant.
        /// </summary>
        /// <param name="jd">Julian Ephemeris Day</param>
        /// <param name="ecl">Geocentrical/topocentrical ecliptical coordinates of the Moon</param>
        /// <param name="deltaPsi">Nutation in longitude, in degrees.</param>
        /// <returns></returns>
        public static Libration Libration(double jd, CrdsEcliptical ecl, double deltaPsi)
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
            F = Angle.ToRadians(Angle.To360(F));

            // Multiplier related to the eccentricity of the Earth orbit
            double E = 1 - 0.002516 * T - 0.0000074 * T2;

            double K1 = 119.75 + 131.849 * T;
            K1 = Angle.ToRadians(K1);

            double K2 = 72.56 + 20.186 * T;
            K2 = Angle.ToRadians(K2);

            // Inclination of the mean lunar equator to ecliptic
            double I = Angle.ToRadians(1.54242);

            double W = Angle.ToRadians(Angle.To360(ecl.Lambda - deltaPsi - Omega));

            double rho =
                -0.02752 * Math.Cos(M_)
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

            double tau =
                0.02520 * E * Math.Sin(M)
                + 0.00473 * Math.Sin(2 * M_ - 2 * F)
                - 0.00467 * Math.Sin(M_)
                + 0.00396 * Math.Sin(K1)
                + 0.00276 * Math.Sin(2 * M_ - 2 * D)
                + 0.00196 * Math.Sin(W)
                - 0.00183 * Math.Cos(M_ - F)
                + 0.00115 * Math.Sin(M_ - 2 * D)
                - 0.00096 * Math.Sin(M_ - D)
                + 0.00046 * Math.Sin(2 * F - 2 * D)
                - 0.00039 * Math.Sin(M_ - F)
                - 0.00032 * E * Math.Sin(M_ - M - D)
                + 0.00027 * E * Math.Sin(2 * M_ - M - 2 * D)
                + 0.00023 * Math.Sin(K2)
                - 0.00014 * Math.Sin(2 * D)
                + 0.00014 * Math.Cos(2 * M_ - 2 * F)
                - 0.00012 * Math.Sin(M_ - 2 * F)
                - 0.00012 * Math.Sin(2 * M_)
                + 0.00011 * E * Math.Sin(2 * M_ - 2 * M - 2 * D);

            double beta = Angle.ToRadians(ecl.Beta);

            double y = Math.Sin(W) * Math.Cos(beta) * Math.Cos(I) - Math.Sin(beta) * Math.Sin(I);
            double x = Math.Cos(W) * Math.Cos(beta);

            double A = Math.Atan2(y, x);

            // optical libration in latitude, in degrees
            double b_ = Angle.ToDegrees(Math.Asin(-Math.Sin(W) * Math.Cos(beta) * Math.Sin(I) - Math.Sin(beta) * Math.Cos(I)));

            // physical libration in latitude, in degrees
            double b__ = sigma * Math.Cos(A) - rho * Math.Sin(A);

            // total libration in latitude, in degrees
            double b = b_ + b__;

            // optical libration in longitude, in degrees
            double l_ = Angle.To360(Angle.ToDegrees(A - F));
            if (l_ > 180) l_ -= 360;

            // physical libration in longitude, in degrees
            double l__ = -tau + (rho * Math.Cos(A) + sigma * Math.Sin(A)) * Math.Tan(Angle.ToRadians(b_));
            if (l__ > 180) l__ -= 360;

            // total libration in longitude, in degrees
            double l = l_ + l__;

            return new Libration() { l = l, b = b };
        }

        /// <summary>
        /// Finds the instant of nearest maximal libration of the Moon
        /// </summary>
        /// <param name="jd">Julian day to start searching from</param>
        /// <param name="edge">Libration edge, i.e. libration direction (north, south, east or west) to look for maximum value.</param>
        /// <param name="angle">Output value of libration anglem in degrees. Note that the value can be negative for southern and western librations.</param>
        /// <returns>Instant of maximal libration nearest to the starting date <paramref name="jd"/>, in julian days.</returns>
        /// <remarks>
        /// The method is based on calculation of mean instant (with known event periodicity and epoch instant), 
        /// then iterative search is used to find the exact instant.
        /// </remarks>
        public static double NearestMaxLibration(double jd, LibrationEdge edge, out double angle)
        {
            // resulting libration angle, should be initialized
            angle = 0;

            // flag to distinguish longitude/latitude librations
            bool isLongitudeLibration = edge == LibrationEdge.East || edge == LibrationEdge.West;

            // periodicity, i.e. librations cycle duration in days
            double P = isLongitudeLibration ? ANOMALISTIC_PERIOD : DRACONIC_PERIOD;

            // libration dates closest to J2000.0 epoch, i.e. librations "epochs" in current context
            double jd0 = new double[] { 2451531.52094581, 2451544.2522909, 2451540.94370721, 2451556.77519409 }[(int)edge];

            // number of libration cycles passed since the epoch
            double k = Math.Round((jd - jd0) / P);

            // mean date of nearest libration, needs to be corrected to find exact date
            double jdLibration = jd0 + k * P;

            // three values of libration angle to find the local extremum
            double[] angles = new double[3];

            // differences in julian days between three time instants
            double[] x = new[] { -0.5, 0, 0.5 };

            // value of X at the extremum point (vertex of parabola)
            double xv = double.MaxValue;

            // accuracy, in day fractions (1 munute of time is enough)
            double minute = TimeSpan.FromMinutes(1).TotalDays;

            // iterative search for nearest libration
            while (Math.Abs(xv) > minute)
            {
                for (int i = 0; i < 3; i++)
                {
                    // julian day at the current point
                    double jdx = jdLibration + x[i];

                    // geocentric ecliptical coordinates of the Moon
                    CrdsEcliptical ecl = LunarMotion.GetCoordinates(jdx);

                    // nutation in longitude for the instant
                    double deltaPsi = Nutation.NutationElements(jdx).deltaPsi;

                    // libration angles for the given instant
                    var libration = Libration(jdx, ecl, deltaPsi);

                    // save angle to the array
                    angles[i] = isLongitudeLibration ? libration.l : libration.b;
                }

                // find the local extremum
                if (angles[1] < 0)
                {
                    Interpolation.FindMinimum(x, angles, 1e-6, out xv, out angle);
                }
                else
                {
                    Interpolation.FindMaximum(x, angles, 1e-6, out xv, out angle);
                }

                // correct libration instant
                jdLibration += xv;
            }

            return jdLibration;
        }

        /// <summary>
        /// Calculates instant of the nearest Moon phase for the given date
        /// </summary>
        /// <param name="jd">Julian Day to calculate nearest phase</param>
        /// <param name="phase">Phase to be found</param>
        /// <returns>Julain Day corrsponding to the instant of nearest lunar phase</returns>
        /// <remarks>
        /// The method is taken from AA(II), chapter 49.
        /// </remarks>
        public static double NearestPhase(double jd, MoonPhase phase)
        {
            Date d = new Date(jd);
            double year = d.Year + (Date.JulianEphemerisDay(d) - Date.JulianDay0(d.Year)) / 365.25;
            double k = Math.Floor((year - 2000) * 12.3685);

            k += (int)phase / 100.0;

            double T = k / 1236.85;
            double T2 = T * T;
            double T3 = T2 * T;
            double T4 = T3 * T;

            double jdMeanPhase = 2451550.09766 + 29.530588861 * k
                                            + 0.00015437 * T2
                                            - 0.000000150 * T3
                                            + 0.00000000073 * T4;

            double M = 2.5534 + 29.10535670 * k
                             - 0.0000014 * T2
                             - 0.00000011 * T3;
            

            double M_ = 201.5643 + 385.81693528 * k
                                 + 0.0107582 * T2
                                 + 0.00001238 * T3
                                 - 0.000000058 * T4;

            double F = 160.7108 + 390.67050284 * k
                                - 0.0016118 * T2
                                - 0.00000227 * T3
                                + 0.000000011 * T4;
            

            double Omega = 124.7746 - 1.56375588 * k
                                    + 0.0020672 * T2
                                    + 0.00000215 * T3;

            M = Angle.ToRadians(M);
            M_ = Angle.ToRadians(M_);
            F = Angle.ToRadians(F);
            Omega = Angle.ToRadians(Omega);

            double[] A = new double[15];

            A[1] = 299.77 + 0.107408 * k - 0.009173 * T2;
            A[2] = 251.88 + 0.016321 * k;
            A[3] = 251.83 + 26.651866 * k;
            A[4] = 349.42 + 36.412478 * k;
            A[5] = 84.66 + 18.206239 * k;
            A[6] = 141.74 + 53.303771 * k;
            A[7] = 207.14 + 2.453732 * k;
            A[8] = 154.84 + 7.306860 * k;
            A[9] = 34.52 + 27.261239 * k;
            A[10] = 207.19 + 0.121824 * k;
            A[11] = 291.34 + 1.844379 * k;
            A[12] = 161.72 + 24.198154 * k;
            A[13] = 239.56 + 25.513099 * k;
            A[14] = 331.55 + 3.592518 * k;

            for (int i = 1; i < 15; i++)
            {
                A[i] = Angle.ToRadians(A[i]);
            }

            double E = 1 - 0.002516 * T - 0.0000074 * T2;

            double addition = 0;

            if (phase == MoonPhase.NewMoon)
            {
                addition =
                    -0.40720 * Math.Sin(M_)
                    + 0.17241 * E * Math.Sin(M)
                    + 0.01608 * Math.Sin(2 * M_)
                    + 0.01039 * Math.Sin(2 * F)
                    + 0.00739 * E * Math.Sin(M_ - M)
                    - 0.00514 * E * Math.Sin(M_ + M)
                    + 0.00208 * E * E * Math.Sin(2 * M)
                    - 0.00111 * Math.Sin(M_ - 2 * F)
                    - 0.00057 * Math.Sin(M_ + 2 * F)
                    + 0.00056 * E * Math.Sin(2 * M_ + M)
                    - 0.00042 * Math.Sin(3 * M_)
                    + 0.00042 * E * Math.Sin(M + 2 * F)
                    + 0.00038 * E * Math.Sin(M - 2 * F)
                    - 0.00024 * E * Math.Sin(2 * M_ - M)
                    - 0.00017 * Math.Sin(Omega)
                    - 0.00007 * Math.Sin(M_ + 2 * M)
                    + 0.00004 * Math.Sin(2 * M_ - 2 * F)
                    + 0.00004 * Math.Sin(3 * M)
                    + 0.00003 * Math.Sin(M_ + M - 2 * F)
                    + 0.00003 * Math.Sin(2 * M_ + 2 * F)
                    - 0.00003 * Math.Sin(M_ + M + 2 * F)
                    + 0.00003 * Math.Sin(M_ - M + 2 * F)
                    - 0.00002 * Math.Sin(M_ - M - 2 * F)
                    - 0.00002 * Math.Sin(3 * M_ + M)
                    + 0.00002 * Math.Sin(4 * M_);
            }

            if (phase == MoonPhase.FullMoon)
            {
                addition =
                    -0.40614 * Math.Sin(M_)
                    + 0.17302 * E * Math.Sin(M)
                    + 0.01614 * Math.Sin(2 * M_)
                    + 0.01043 * Math.Sin(2 * F)
                    + 0.00734 * E * Math.Sin(M_ - M)
                    - 0.00515 * E * Math.Sin(M_ + M)
                    + 0.00209 * E * E * Math.Sin(2 * M)
                    - 0.00111 * Math.Sin(M_ - 2 * F)
                    - 0.00057 * Math.Sin(M_ + 2 * F)
                    + 0.00056 * E * Math.Sin(2 * M_ + M)
                    - 0.00042 * Math.Sin(3 * M_)
                    + 0.00042 * E * Math.Sin(M + 2 * F)
                    + 0.00038 * E * Math.Sin(M - 2 * F)
                    - 0.00024 * E * Math.Sin(2 * M_ - M)
                    - 0.00017 * Math.Sin(Omega)
                    - 0.00007 * Math.Sin(M_ + 2 * M)
                    + 0.00004 * Math.Sin(2 * M_ - 2 * F)
                    + 0.00004 * Math.Sin(3 * M)
                    + 0.00003 * Math.Sin(M_ + M - 2 * F)
                    + 0.00003 * Math.Sin(2 * M_ + 2 * F)
                    - 0.00003 * Math.Sin(M_ + M + 2 * F)
                    + 0.00003 * Math.Sin(M_ - M + 2 * F)
                    - 0.00002 * Math.Sin(M_ - M - 2 * F)
                    - 0.00002 * Math.Sin(3 * M_ + M)
                    + 0.00002 * Math.Sin(4 * M_);
            }

            if (phase == MoonPhase.FirstQuarter ||
                phase == MoonPhase.LastQuarter)
            {
                addition =
                    -0.62801 * Math.Sin(M_)
                    + 0.17172 * E * Math.Sin(M)
                    - 0.01183 * E * Math.Sin(M_ + M)
                    + 0.00862 * Math.Sin(2 * M_)
                    + 0.00804 * Math.Sin(2 * F)
                    + 0.00454 * E * Math.Sin(M_ - M)
                    + 0.00204 * E * E * Math.Sin(2 * M)
                    - 0.00180 * Math.Sin(M_ - 2 * F)
                    - 0.00070 * Math.Sin(M_ + 2 * F)
                    - 0.00040 * Math.Sin(3 * M_)
                    - 0.00034 * E * Math.Sin(2 * M_ - M)
                    + 0.00032 * E * Math.Sin(M + 2 * F)
                    + 0.00032 * E * Math.Sin(M - 2 * F)
                    - 0.00028 * E * E * Math.Sin(M_ + 2 * M)
                    + 0.00027 * E * Math.Sin(2 * M_ + M)
                    - 0.00017 * Math.Sin(Omega)
                    - 0.00005 * Math.Sin(M_ - M - 2 * F)
                    + 0.00004 * Math.Sin(2 * M_ + 2 * F)
                    - 0.00004 * Math.Sin(M_ + M + 2 * F)
                    + 0.00004 * Math.Sin(M_ - 2 * M)
                    + 0.00003 * Math.Sin(M_ + M - 2 * F)
                    + 0.00003 * Math.Sin(3 * M)
                    + 0.00002 * Math.Sin(2 * M_ - 2 * F)
                    + 0.00002 * Math.Sin(M_ - M + 2 * F)
                    - 0.00002 * Math.Sin(3 * M_ + M);

                double W = 0.00306 - 0.00038 * E * Math.Cos(M) + 0.00026 * Math.Cos(M_)
                    - 0.00002 * Math.Cos(M_ - M) + 0.00002 * Math.Cos(M_ + M) + 0.00002 * Math.Cos(2 * F);

                if (phase == MoonPhase.FirstQuarter) addition += W;
                if (phase == MoonPhase.LastQuarter) addition -= W;
            }

            double correction =
                  0.000325 * Math.Sin(A[1])
                + 0.000165 * Math.Sin(A[2])
                + 0.000164 * Math.Sin(A[3])
                + 0.000126 * Math.Sin(A[4])
                + 0.000110 * Math.Sin(A[5])
                + 0.000062 * Math.Sin(A[6])
                + 0.000060 * Math.Sin(A[7])
                + 0.000056 * Math.Sin(A[8])
                + 0.000047 * Math.Sin(A[9])
                + 0.000042 * Math.Sin(A[10])
                + 0.000040 * Math.Sin(A[11])
                + 0.000037 * Math.Sin(A[12])
                + 0.000035 * Math.Sin(A[13])
                + 0.000023 * Math.Sin(A[14]);

            jdMeanPhase += addition + correction;

            return jdMeanPhase;
        }

        /// <summary>
        /// Calculates instant of the nearest Moon apsis (perigee or apogee) for the given date
        /// </summary>
        /// <param name="jd">Julian Day to calculate nearest apsis</param>
        /// <param name="apsis">Apsis to be found</param>
        /// <returns>Julain Day corrsponding to the instant of nearest apsis</returns>
        /// <remarks>
        /// The method is taken from AA(II), chapter 50.
        /// </remarks>
        public static double NearestApsis(double jd, MoonApsis apsis, out double diameter)
        {
            Date d = new Date(jd);
            double year = d.Year + (Date.JulianEphemerisDay(d) - Date.JulianDay0(d.Year)) / 365.25;

            double k = Math.Floor((year - 1999.97) * 13.2555);
            k += (double)apsis / 10.0;

            double T = k / 1325.55;
            double T2 = T * T;
            double T3 = T2 * T;
            double T4 = T3 * T;

            double jdMean = 2451534.6698 + 27.55454989 * k
                                      - 0.0006691 * T2
                                      - 0.000001098 * T3
                                      + 0.0000000052 * T4;

            double D = 171.9179 + 335.9106046 * k
                                - 0.0100383 * T2
                                - 0.00001156 * T3
                                + 0.000000055 * T4;

            double M = 347.3477 + 27.1577721 * k
                                - 0.0008130 * T2
                                - 0.0000010 * T3;

            double F = 316.6109 + 364.5287911 * k
                                - 0.0125053 * T2
                                - 0.0000148 * T3;

            D = Angle.To360(D);
            M = Angle.To360(M);
            F = Angle.To360(F);

            D = Angle.ToRadians(D);
            M = Angle.ToRadians(M);            
            F = Angle.ToRadians(F);

            double terms = 0;
            double parallax = 0;

            if (apsis == MoonApsis.Perigee)
            {
                terms =
                    Math.Sin(2 * D) * (-1.6769) +
                    Math.Sin(4 * D) * (+0.4589) +
                    Math.Sin(6 * D) * (-0.1856) +
                    Math.Sin(8 * D) * (+0.0883) +
                    Math.Sin(2 * D - M) * (-0.0773 + 0.00019 * T) +
                    Math.Sin(M) * (+0.0502 - 0.00013 * T) +
                    Math.Sin(10 * D) * (-0.0460) +
                    Math.Sin(4 * D - M) * (+0.0422 - 0.00011 * T) +
                    Math.Sin(6 * D - M) * (-0.0256) +
                    Math.Sin(12 * D) * (+0.0253) +
                    Math.Sin(D) * (+0.0237) +
                    Math.Sin(8 * D - M) * (+0.0162) +
                    Math.Sin(14 * D) * (-0.0145) +
                    Math.Sin(2 * F) * (+0.0129) +
                    Math.Sin(3 * D) * (-0.0112) +
                    Math.Sin(10 * D - M) * (-0.0104) +
                    Math.Sin(16 * D) * (+0.0086) +
                    Math.Sin(12 * D - M) * (+0.0069) +
                    Math.Sin(5 * D) * (+0.0066) +
                    Math.Sin(2 * D + 2 * F) * (-0.0053) +
                    Math.Sin(18 * D) * (-0.0052) +
                    Math.Sin(14 * D - M) * (-0.0046) +
                    Math.Sin(7 * D) * (-0.0041) +
                    Math.Sin(2 * D + M) * (+0.0040) +
                    Math.Sin(20 * D) * (+0.0032) +
                    Math.Sin(D + M) * (-0.0032) +
                    Math.Sin(16 * D - M) * (+0.0031) +
                    Math.Sin(4 * D + M) * (-0.0029) +
                    Math.Sin(9 * D) * (+0.0027) +
                    Math.Sin(4 * D + 2 * F) * (+0.0027) +
                    Math.Sin(2 * D - 2 * M) * (-0.0027) +
                    Math.Sin(4 * D - 2 * M) * (+0.0024) +
                    Math.Sin(6 * D - 2 * M) * (-0.0021) +
                    Math.Sin(22 * D) * (-0.0021) +
                    Math.Sin(18 * D - M) * (-0.0021) +
                    Math.Sin(6 * D + M) * (+0.0019) +
                    Math.Sin(11 * D) * (-0.0018) +
                    Math.Sin(8 * D + M) * (-0.0014) +
                    Math.Sin(4 * D - 2 * F) * (-0.0014) +
                    Math.Sin(6 * D + 2 * F) * (-0.0014) +
                    Math.Sin(3 * D + M) * (+0.0014) +
                    Math.Sin(5 * D + M) * (-0.0014) +
                    Math.Sin(13 * D) * (+0.0013) +
                    Math.Sin(20 * D - M) * (+0.0013) +
                    Math.Sin(3 * D + 2 * M) * (+0.0011) +
                    Math.Sin(4 * D + 2 * F - 2 * M) * (-0.0011) +
                    Math.Sin(D + 2 * M) * (-0.0010) +
                    Math.Sin(22 * D - M) * (-0.0009) +
                    Math.Sin(4 * F) * (-0.0008) +
                    Math.Sin(6 * D - 2 * F) * (+0.0008) +
                    Math.Sin(2 * D - 2 * F + M) * (+0.0008) +
                    Math.Sin(2 * M) * (+0.0007) +
                    Math.Sin(2 * F - M) * (+0.0007) +
                    Math.Sin(2 * D + 4 * F) * (+0.0007) +
                    Math.Sin(2 * F - 2 * M) * (-0.0006) +
                    Math.Sin(2 * D - 2 * F + 2 * M) * (-0.0006) +
                    Math.Sin(24 * D) * (+0.0006) +
                    Math.Sin(4 * D - 4 * F) * (+0.0005) +
                    Math.Sin(2 * D + 2 * M) * (+0.0005) +
                    Math.Sin(D - M) * (-0.0004);

                parallax =
                    3629.215
                    + 63.224 * Math.Cos(2 * D)
                    - 6.990 * Math.Cos(4 * D)
                    + 2.834 * Math.Cos(2 * D - M)
                    - 0.0071 * T * Math.Cos(2 * D - M)
                    + 1.927 * Math.Cos(6 * D)
                    - 1.263 * Math.Cos(D)
                    - 0.702 * Math.Cos(8 * D)
                    + 0.696 * Math.Cos(M)
                    - 0.0017 * T * Math.Cos(M)
                    - 0.690 * Math.Cos(2 * F)
                    - 0.629 * Math.Cos(4 * D - M)
                    + 0.0016 * T * Math.Cos(4 * D - M)
                    - 0.392 * Math.Cos(2 * D - 2 * F)
                    + 0.297 * Math.Cos(10 * D)
                    + 0.260 * Math.Cos(6 * D - M)
                    + 0.201 * Math.Cos(3 * D)
                    - 0.161 * Math.Cos(2 * D + M)
                    + 0.157 * Math.Cos(D + M)
                    - 0.138 * Math.Cos(12 * D)
                    - 0.127 * Math.Cos(8 * D - M)
                    + 0.104 * Math.Cos(2 * D + 2 * F)
                    + 0.104 * Math.Cos(2 * D - 2 * M)
                    - 0.079 * Math.Cos(5 * D)
                    + 0.068 * Math.Cos(14 * D)
                    + 0.067 * Math.Cos(10 * D - M)
                    + 0.054 * Math.Cos(4 * D + M)
                    - 0.038 * Math.Cos(12 * D - M)
                    - 0.038 * Math.Cos(4 * D - 2 * M)
                    + 0.037 * Math.Cos(7 * D)
                    - 0.037 * Math.Cos(4 * D + 2 * F)
                    - 0.035 * Math.Cos(16 * D)
                    - 0.030 * Math.Cos(3 * D + M)
                    + 0.029 * Math.Cos(D - M)
                    - 0.025 * Math.Cos(6 * D + M)
                    + 0.023 * Math.Cos(2 * M)
                    + 0.023 * Math.Cos(14 * D - M)
                    - 0.023 * Math.Cos(2 * D + 2 * M)
                    + 0.022 * Math.Cos(6 * D - 2 * M)
                    - 0.021 * Math.Cos(2D - 2 * F - M)
                    - 0.020 * Math.Cos(9 * D)
                    + 0.019 * Math.Cos(18 * D)
                    + 0.017 * Math.Cos(6 * D + 2 * F)
                    + 0.014 * Math.Cos(2 * F - M)
                    - 0.014 * Math.Cos(16 * D - M)
                    + 0.013 * Math.Cos(4 * D - 2 * F)
                    + 0.012 * Math.Cos(8 * D + M)
                    + 0.011 * Math.Cos(11 * D)
                    + 0.010 * Math.Cos(5 * D + M)
                    - 0.010 * Math.Cos(20 * D);
            }
            else if (apsis == MoonApsis.Apogee)
            {
                terms =
                    Math.Sin(2 * D) * (+0.4392) +
                    Math.Sin(4 * D) * (+0.0684) +
                    Math.Sin(M) * (+0.0456 - 0.00011 * T) +
                    Math.Sin(2 * D - M) * (+0.0426 - 0.00011 * T) +
                    Math.Sin(2 * F) * (+0.0212) +
                    Math.Sin(D) * (-0.0189) +
                    Math.Sin(6 * D) * (+0.0144) +
                    Math.Sin(4 * D - M) * (+0.0113) +
                    Math.Sin(2 * D + 2 * F) * (+0.0047) +
                    Math.Sin(D + M) * (+0.0036) +
                    Math.Sin(8 * D) * (+0.0035) +
                    Math.Sin(6 * D - M) * (+0.0034) +
                    Math.Sin(2 * D - 2 * F) * (-0.0034) +
                    Math.Sin(2 * D - 2 * M) * (+0.0022) +
                    Math.Sin(3 * D) * (-0.0017) +
                    Math.Sin(4 * D + 2 * F) * (+0.0013) +
                    Math.Sin(8 * D - M) * (+0.0011) +
                    Math.Sin(4 * D - 2 * M) * (+0.0010) +
                    Math.Sin(10 * D) * (+0.0009) +
                    Math.Sin(3 * D + M) * (+0.0007) +
                    Math.Sin(2 * M) * (+0.0006) +
                    Math.Sin(2 * D + M) * (+0.0005) +
                    Math.Sin(2 * D + 2 * M) * (+0.0005) +
                    Math.Sin(6 * D + 2 * F) * (+0.0004) +
                    Math.Sin(6 * D - 2 * M) * (+0.0004) +
                    Math.Sin(10 * D - M) * (+0.0004) +
                    Math.Sin(5 * D) * (-0.0004) +
                    Math.Sin(4 * D - 2 * F) * (-0.0004) +
                    Math.Sin(2 * F + M) * (+0.0003) +
                    Math.Sin(12 * D) * (+0.0003) +
                    Math.Sin(2 * D + 2 * F - M) * (+0.0003) +
                    Math.Sin(D - M) * (-0.0003);

                parallax =
                    3245.251
                    - 9.147 * Math.Cos(2 * D)
                    - 0.841 * Math.Cos(D)
                    + 0.697 * Math.Cos(2 * F)
                    - 0.656 * Math.Cos(M)
                    + 0.0016 * T * Math.Cos(M)
                    + 0.355 * Math.Cos(4 * D)
                    + 0.159 * Math.Cos(2 * D - M)
                    + 0.127 * Math.Cos(D + M)
                    + 0.065 * Math.Cos(4 * D - M)
                    + 0.052 * Math.Cos(6 * D)
                    + 0.043 * Math.Cos(2 * D + M)
                    + 0.031 * Math.Cos(2 * D + 2 * F)
                    - 0.023 * Math.Cos(2 * D - 2 * F)
                    + 0.022 * Math.Cos(2 * D - 2 * M)
                    + 0.019 * Math.Cos(2 * D + 2 * M)
                    - 0.016 * Math.Cos(2 * M)
                    + 0.014 * Math.Cos(6 * D - M)
                    + 0.010 * Math.Cos(8 * D);
            }

            jdMean += terms;

            double distance = 6378.14 / Math.Sin(Angle.ToRadians(parallax / 3600.0));
            diameter = 2 * Semidiameter(distance) / 3600;

            return jdMean;
        }

        /// <summary>
        /// Finds the instant of nearest maximal declination (northern or southern) of the Moon
        /// </summary>
        /// <param name="jd">Julian day to start searching from</param>
        /// <param name="declination">Declination direction to search, northern or southern.</param>
        /// <param name="delta">Output value of the maximal declination value.</param>
        /// <returns>Instant of maximal declination of the Moon nearest to the starting date <paramref name="jd"/>, in julian days.</returns>
        /// <remarks>The method is taken from AA(II), chapter 52.</remarks>
        public static double NearestMaxDeclination(double jd, MoonDeclination declination, out double delta)
        {
            Date d = new Date(jd);
            double year = d.Year + (Date.JulianEphemerisDay(d) - Date.JulianDay0(d.Year)) / 365.25;
            double k = Math.Round((year - 2000.03) * 13.3686);

            double T = k / 1336.86;
            double T2 = T * T;
            double T3 = T2 * T;
            double T4 = T3 * T;

            double D = new[] { 152.2029, 345.6676 }[(int)declination] 
                + 333.0705546 * k - 0.0004214 * T2 + 0.00000011 * T3;

            double M = new[] { 14.8591, 1.3951 }[(int)declination] 
                + 26.9281592 * k - 0.00003555 * T2 - 0.00000010 * T3;

            double M_ = new[] { 4.6881, 186.2100 }[(int)declination] 
                + 356.9562794 * k + 0.0103066 * T2 + 0.00001251 * T3;

            double F = new[] { 325.8867, 145.1633 }[(int)declination] 
                + 1.4467807 * k - 0.0020690 * T2 - 0.00000215 * T3;

            double E = 1 - 0.002516 * T - 0.0000047 * T2;

            D = Angle.To360(D);
            M = Angle.To360(M);
            M_ = Angle.To360(M_);
            F = Angle.To360(F);

            D = Angle.ToRadians(D);
            M = Angle.ToRadians(M);
            M_ = Angle.ToRadians(M_);
            F = Angle.ToRadians(F);

            double jdMean = new[] { 2451562.5897, 2451548.9289 }[(int)declination] 
                + 27.321582247 * k + 0.000119804 * T2 - 0.000000141 * T3;

            double[] cTime, cValue;
            double[] aTime, aValue;
            Func<double, double>[] fTime, fValue;

            if (declination == MoonDeclination.North)
            {
                cTime = new[] { 0.8975, -0.4726, -0.1030, -0.0976, -0.0462, -0.0461, -0.0438, 0.0162 * E, -0.0157, 0.0145,
                    0.0136, -0.0095, -0.0091, -0.0089, 0.0075, -0.0068, 0.0061, -0.0047, -0.0043 * E, -0.0040, -0.0037,
                    0.0031, 0.0030, -0.0029, -0.0029 * E, -0.0027, 0.0024 * E, -0.0021, 0.0019, 0.0018, 0.0018, 0.0017,
                    0.0017, -0.0014, 0.0013, 0.0013, 0.0012, 0.0011, -0.0011, 0.0010, 0.0010 * E, -0.0009, 0.0007, -0.0007 };
                aTime = new[] { F, M_, 2 * F, 2 * D - M_, M_ - F, M_ + F, 2 * D, M, 3 * F, M_ + 2 * F, 2 * D - F,
                    2 * D - M_ - F, 2 * D - M_ + F, 2 * D + F, 2 * M_, M_ - 2 * F, 2 * M_ - F, M_ + 3 * F, 2 * D - M - M_,
                    M_ - 2 * F, 2 * D - 2 * M_, F, 2 * D + M_, M_ + 2 * F, 2 * D - M, M_ + F, M - M_, M_ - 3 * F, 2 * M_ + F,
                    2 * D - 2 * M_ - F, 3 * F, M_ + 3 * F, 2 * M_, 2 * D - M_, 2 * D + M_ + F, M_, 3 * M_ + F, 2 * D - M_ + F,
                    2 * D - 2 * M_, D + F, M + M_, 2 * D - 2 * F, 2 * M_ + F, 3 * M_ + F };
                fTime = new Func<double, double>[] { Math.Cos, Math.Sin, Math.Sin, Math.Sin, Math.Cos, Math.Cos, Math.Sin,
                    Math.Sin, Math.Cos, Math.Sin, Math.Cos, Math.Cos, Math.Cos, Math.Cos, Math.Sin, Math.Sin, Math.Cos,
                    Math.Sin, Math.Sin, Math.Cos, Math.Sin, Math.Sin, Math.Sin, Math.Cos, Math.Sin, Math.Sin, Math.Sin,
                    Math.Sin, Math.Sin, Math.Cos, Math.Sin, Math.Cos, Math.Cos, Math.Cos, Math.Cos, Math.Cos, Math.Sin,
                    Math.Sin, Math.Cos, Math.Cos, Math.Sin, Math.Sin, Math.Cos, Math.Cos };

                cValue = new[] {5.1093, 0.2658, 0.1448, -0.0322, 0.0133, 0.0125, -0.0124, -0.0101, 0.0097, -0.0087 * E,
                    0.0074, 0.0067, 0.0063, 0.0060 * E, -0.0057, -0.0056, 0.0052, 0.0041, -0.0040, 0.0038, -0.0034,
                    -0.0029, 0.0029, -0.0028 * E, -0.0028, -0.0023, -0.0021, 0.0019, 0.0018, 0.0017, 0.0015, 0.0014,
                    -0.0012, -0.0012, -0.0010, -0.0010, 0.0006 };
                aValue = new[]{F, 2 * F, 2 * D - F, 3 * F, 2 * D - 2 * F, 2 * D, M_ - F, M_ + 2 * F, F, 2 * D + M - F,
                    M_ + 3 * F, D + F, M_ - 2 * F, 2 * D - M - F, 2 * D - M_ - F, M_ + F, M_ + 2 * F, 2 * M_ + F,
                    M_ - 3 * F, 2 * M_ - F, M_ - 2 * F, 2 * M_, 3 * M_ + F, 2 * D + M - F, M_ - F, 3 * F, 2 * D + F,
                    M_ + 3 * F, D + F, 2 * M_ - F, 3 * M_ + F, 2 * D + 2 * M_ + F, 2 * D - 2 * M_ - F, 2 * M_, M_, 2 * F, M_ + F };
                fValue = new Func<double, double>[] {Math.Sin, Math.Cos, Math.Sin, Math.Sin, Math.Cos, Math.Cos,
                    Math.Sin, Math.Sin, Math.Cos, Math.Sin, Math.Sin, Math.Sin, Math.Sin, Math.Sin, Math.Sin,
                    Math.Cos, Math.Cos, Math.Cos, Math.Cos, Math.Cos, Math.Cos, Math.Sin, Math.Sin, Math.Cos,
                    Math.Cos, Math.Cos, Math.Sin, Math.Cos, Math.Cos, Math.Sin, Math.Cos, Math.Cos, Math.Sin,
                    Math.Cos, Math.Cos, Math.Sin, Math.Sin };
            }
            else
            {
                cTime = new[] {-0.8975, -0.4726, -0.1030, -0.0976, 0.0541, 0.0516, -0.0438, 0.0112 * E, 0.0157,
                    0.0023, -0.0136, 0.0110, 0.0091, 0.0089, 0.0075, -0.0030, -0.0061, -0.0047, -0.0043 * E, 0.0040,
                    -0.0037, -0.0031, 0.0030, 0.0029, -0.0029 * E, -0.0027, 0.0024 * E, -0.0021, -0.0019,
                    -0.0006, -0.0018, -0.0017, 0.0017, 0.0014, -0.0013, -0.0013, 0.0012, 0.0011, 0.0011,
                    0.0010, 0.0010 * E, -0.0009, -0.0007, -0.0007 };
                aTime = new[] {F, M_, 2 * F, 2 * D - M_, M_ - F, M_ + F, 2 * D, M, 3 * F, M_ + 2 * F, 2 * D - F,
                    2 * D - M_ - F, 2 * D - M_ + F, 2 * D + F, 2 * M_, M_ - 2 * F, 2 * M_ - F, M_ + 3 * F,
                    2 * D - M - M_, M_ - 2 * F, 2 * D - 2 * M_, F, 2 * D + M_, M_ + 2 * F, 2 * D - M, M_ + F,
                    M - M_, M_ - 3 * F, 2 * M_ + F, 2 * D - 2 * M_ - F, 3 * F, M_ + 3 * F, 2 * M_, 2 * D - M_,
                    2 * D + M_ + F, M_, 3 * M_ + F, 2 * D - M_ + F, 2 * D - 2 * M_, D + F, M + M_, 2 * D - 2 * F,
                    2 * M_ + F, 3 * M_ + F };
                fTime = new Func<double, double>[] { Math.Cos, Math.Sin, Math.Sin, Math.Sin, Math.Cos, Math.Cos,
                    Math.Sin, Math.Sin, Math.Cos, Math.Sin, Math.Cos, Math.Cos, Math.Cos, Math.Cos, Math.Sin,
                    Math.Sin, Math.Cos, Math.Sin, Math.Sin, Math.Cos, Math.Sin, Math.Sin, Math.Sin, Math.Cos,
                    Math.Sin, Math.Sin, Math.Sin, Math.Sin, Math.Sin, Math.Cos, Math.Sin, Math.Cos, Math.Cos,
                    Math.Cos, Math.Cos, Math.Cos, Math.Sin, Math.Sin, Math.Cos, Math.Cos,
                    Math.Sin, Math.Sin, Math.Cos, Math.Cos };

                cValue = new[] {-5.1093, 0.2658, -0.1448, 0.0322, 0.0133, 0.0125, -0.0015, 0.0101, -0.0097,
                    0.0087 * E, 0.0074, 0.0067, -0.0063, -0.0060 * E, 0.0057, -0.0056, -0.0052, -0.0041,
                    -0.0040, -0.0038, 0.0034, -0.0029, 0.0029, 0.0028 * E, -0.0028, 0.0023, 0.0021, 0.0019,
                    0.0018, -0.0017, 0.0015, 0.0014, 0.0012, -0.0012, 0.0010, -0.0010, 0.0037 };
                aValue = new[] {F, 2 * F, 2 * D - F, 3 * F, 2 * D - 2 * F, 2 * D, M_ - F, M_ + 2 * F, F,
                    2 * D + M - F, M_ + 3 * F, D + F, M_ - 2 * F, 2 * D - M - F, 2 * D - M_ - F,
                    M_ + F, M_ + 2 * F, 2 * M_ + F, M_ - 3 * F, 2 * M_ - F, M_ - 2 * F, 2 * M_,
                    3 * M_ + F, 2 * D + M - F, M_ - F, 3 * F, 2 * D + F, M_ + 3 * F, D + F, 2 * M_ - F,
                    3 * M_ + F, 2 * D + 2 * M_ + F, 2 * D - 2 * M_ - F, 2 * M_, M_, 2 * F, M_ + F };
                fValue = new Func<double, double>[] {Math.Sin, Math.Cos, Math.Sin, Math.Sin, Math.Cos,
                    Math.Cos, Math.Sin, Math.Sin, Math.Cos, Math.Sin, Math.Sin, Math.Sin, Math.Sin,
                    Math.Sin, Math.Sin, Math.Cos, Math.Cos, Math.Cos, Math.Cos, Math.Cos, Math.Cos,
                    Math.Sin, Math.Sin, Math.Cos, Math.Cos, Math.Cos, Math.Sin, Math.Cos, Math.Cos,
                    Math.Sin, Math.Cos, Math.Cos, Math.Sin, Math.Cos, Math.Cos, Math.Sin, Math.Sin };
            }

            double timeTerms = 0;
            for (int i = 0; i < aTime.Length; i++)
            {
                timeTerms += cTime[i] * fTime[i](aTime[i]);
            }

            double valueTerms = 0;
            for (int i = 0; i < aValue.Length; i++)
            {
                valueTerms += cValue[i] * fValue[i](aValue[i]);
            }

            jdMean += timeTerms;

            delta = 23.6961 - 0.013004 * T + valueTerms;

            return jdMean;
        }

        /// <summary>
        /// Gets age of the Moon (time since last new moon in days)
        /// </summary>
        /// <param name="jd">Julian day to get the Moon age</param>
        /// <returns>Time since last new moon in days</returns>
        /// <remarks>
        /// This method based on calculation of the instant of new moon (see <see cref="NearestPhase(double, MoonPhase)"/> for details).
        /// If calculated nearest date of the new moon is in the future, then we should calculate the previous date of the new moon
        /// by subtracting amount of days (synodic period of the Moon expressed in days, i.e. <see cref="SINODIC_PERIOD"/>) from the date of calculation.
        /// </remarks>
        // TODO: tests
        public static double Age(double jd)
        {
            double jdNM = NearestPhase(jd, MoonPhase.NewMoon);
            if (jd < jdNM)
            {
                jdNM = NearestPhase(jd - SINODIC_PERIOD, MoonPhase.NewMoon);
            }
            return jd - jdNM;
        }

        /// <summary>
        /// Gets magnitude of the Moon by its phase angle.
        /// </summary>
        /// <param name="phaseAngle">Phase angle value, in degrees, from 0 to 180.</param>
        /// <returns>Moon magnitude</returns>
        /// <remarks>
        /// Formula is taken from <see href="https://astronomy.stackexchange.com/questions/10246/is-there-a-simple-analytical-formula-for-the-lunar-phase-brightness-curve"/>
        /// </remarks>
        // TODO: tests
        public static float Magnitude(double phaseAngle)
        {
            double psi = Angle.ToRadians(phaseAngle);
            double psi4 = Math.Pow(psi, 4);

            return (float)(-12.73 + 1.49 * Math.Abs(psi) + 0.043 * psi4);
        }

        /// <summary>
        /// Gets longitude of mean ascending node of Lunar orbit for given instant.
        /// </summary>
        /// <param name="jd">Julian Day</param>
        /// <returns>Longitude of mean ascending node of Lunar orbit, in degrees.</returns>
        // TODO: tests
        public static double MeanAscendingNode(double jd)
        {
            return AscendingNode(jd, trueAscendingNode: false);
        }

        /// <summary>
        /// Gets longitude of true ascending node of Lunar orbit for given instant.
        /// </summary>
        /// <param name="jd">Julian Day</param>
        /// <returns>Longitude of true ascending node of Lunar orbit, in degrees.</returns>
        // TODO: tests
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
        // TODO: tests
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

        /// <summary>
        /// Gets details of Earth shadow for given date.
        /// </summary>
        /// <param name="jd">Julian day to cacluate details.</param>
        /// <returns>Instance of <see cref="ShadowAppearance"/>.</returns>
        /// <remarks>This method is taken from AA(II), chapter 54.</remarks>
        // TODO: tests
        public static ShadowAppearance Shadow(double jd)
        {
            double T = (jd - 2451545.0) / 36525.0;
            double T2 = T * T;
            double T3 = T2 * T;
            double T4 = T3 * T;

            // Sun's mean anomaly (formula 49.4)
            double M = 357.5291092 + 35999.0502909 * T - 0.0001536 * T2 + T3 / 24490000.0;
            M = Angle.To360(M);
            M = Angle.ToRadians(M);

            // Moon's mean anomaly (formula 49.5)
            double M_ = 134.9633964 + 477198.8675055 * T + 0.0087414 * T2 + T3 / 69699.0 - T4 / 14712000.0;
            M_ = Angle.To360(M_);
            M_ = Angle.ToRadians(M_);

            // Multiplier related to the eccentricity of the Earth orbit (formula 47.6)
            double E = 1 - 0.002516 * T - 0.0000074 * T2;

            // p. 381
            double u = 0.0059
                + 0.0046 * E * Math.Cos(M)
                - 0.0182 * Math.Cos(M_)
                + 0.0004 * Math.Cos(2 * M_)
                - 0.0005 * Math.Cos(M + M_);

            return new ShadowAppearance(u);
        }

        /// <summary>
        /// Calculates lunation number for Julian day
        /// </summary>
        /// <param name="jd">Julian day of new moon instant to calculate lunation number.</param>
        /// <param name="system">Lunation system. Brown system is default.</param>
        /// <returns>Lunation number.</returns>
        public static int Lunation(double jd, LunationSystem system = LunationSystem.Brown)
        {
            // Meeus' Lunation Number epoch
            // is a first New Moon of 2000 (6th January, ~ 18:14 UTC)
            double jd0 = 2451550.25972;
            int LN = (int)Math.Round((jd - jd0) / SINODIC_PERIOD);

            switch (system)
            {
                default:
                case LunationSystem.Brown:
                    return LN + 953;
                case LunationSystem.Meeus:
                    return LN;
                case LunationSystem.Goldstine:
                    return LN + 37105;
                case LunationSystem.Hebrew:
                    return LN + 71234;
                case LunationSystem.Islamic:
                    return LN + 17038;
                case LunationSystem.Thai:
                    return LN + 16843;
            }
        }

        /// <summary>
        /// Calculates parameter Q describing the visibility of lunar crescent.
        /// Used for calculating noumenia and epimenia events, and based on work of B.D.Yallop:
        /// see https://webspace.science.uu.nl/~gent0113/islam/downloads/naotn_69.pdf
        /// </summary>
        /// <param name="eclMoon">Geocentric ecliptical coodinates of the Moon.</param>
        /// <param name="eclSun">Geocentric ecliptical coodinates of the Sun.</param>
        /// <param name="moonSemidiameter">Moon semidiameter, in seconds of arc.</param>
        /// <param name="epsilon">True obliquity.</param>
        /// <param name="siderealTime">Sidereal time at Greenwich, in degrees.</param>
        /// <param name="geo">Geographical location of point of interest.</param>
        /// <returns>Value of Q (see referenced work for more details)</returns>
        public static double CrescentQ(CrdsEcliptical eclMoon, CrdsEcliptical eclSun, double moonSemidiameter, double epsilon, double siderealTime, CrdsGeographical geo)
        {
            CrdsHorizontal hSun = eclSun.ToEquatorial(epsilon).ToHorizontal(geo, siderealTime);
            CrdsHorizontal hMoon = eclMoon.ToEquatorial(epsilon).ToHorizontal(geo, siderealTime);

            // Lunar semidiameter, in degrees
            double sd = moonSemidiameter / 3600;

            // difference in azimuths, in degrees
            double DAZ = hSun.Azimuth - hMoon.Azimuth;

            // Arc of Light (angular separation between Sun and Moon, as seen from Earth center), in radians
            double ARCL = Angle.ToRadians(Angle.Separation(eclSun, eclMoon));

            // Arc of Vision, in degrees
            // ARCV is the geocentric difference in altitude between the centre of the Sun 
            // and the centre of the Moon for a given latitude and longitude ignoring the effects of refraction
            double ARCV = Angle.ToDegrees(Math.Acos(Math.Cos(ARCL) / Math.Cos(Angle.ToRadians(DAZ))));
            
            // Width of lunar crescent, in minutes of arc
            double W = sd * (1 - Math.Cos(ARCL)) * 60;
            double W2 = W * W, W3 = W * W2;

            return (ARCV - 11.8371 + 6.3226 * W - 0.7319 * W2 + 0.1018 * W3) / 10;
        }
    }
}
