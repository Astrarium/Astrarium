using System;

namespace ADK
{
    public static class PlanetEphem
    {
        #region Terms

        /// <summary>
        /// Planet semidiameters, in arcseconds, from distance 1 A.U.
        /// </summary>
        private static readonly double[] s0 = new double[] { 3.36, 8.41, 0, 4.68, 98.44, 82.73, 35.02, 33.5, 2.07 };

        /// <summary>
        /// Terms needed for calculation of direction of planet north pole (Right Ascension)
        /// </summary>
        private static readonly double[][] cAlpha0 = new double[][]
        {
            new [] { 281.02, 0.033, 0.276 },     // Mercury
            new [] { 272.78, 0, -0.043 },        // Venus
            new [] { 0.0, 0, 0 },                // Earth
            new [] { 317.681, -0.108, 0.786 },   // Mars
            new [] { 286.05, -0.009, 0.116 },    // Jupiter
            new [] { 40.66, -0.036, 4.731 },     // Saturn
            new [] { 257.43, 0, 1.429 },         // Uranus
            new [] { 295.33, 0, 0.849 },         // Neptune
            new [] { 311.63, 0, 1.250 }          // Pluto
        };

        /// <summary>
        /// Terms needed for calculation of direction of planet north pole (Declination)
        /// </summary>
        private static readonly double[][] cDelta0 = new double[][]
        {
            new [] { 61.45, -0.005, 0.107 },     // Mercury
            new [] { 67.21, 0, 0.027 },          // Venus
            new [] { 0.0, 0, 0 },                // Earth
            new [] { 52.886, -0.061, 0.413 },    // Mars    
            new [] { 64.49, 0.003, -0.018 },     // Jupiter
            new [] { 83.52, -0.004, 0.407 },     // Saturn
            new [] { -15.10, 0, -0.114 },        // Uranus
            new [] { 40.65, 0, 0.242 },          // Neptune
            new [] { 4.18, 0, 0.374 }            // Pluto
        };

        /// <summary>
        /// Terms needed for calculation of position of the null meridian for planets
        /// </summary>
        private static readonly double[][] cW = new double[][]
        {
            new [] { 329.71, 6.1385025, 1.145 },        // Mercury
            new [] { 159.91, -1.14814205, 1.436 },      // Venus
            new [] { 0.0, 0, 0 },                       // Earth
            new [] { 176.655, 350.8919830, 0.620 },     // Mars    
            new [] { 43.30, 870.27, 1.291 },            // Jupiter (System II)
            new [] { 227.2037, 844.3, -3.470  },        // Saturn (System I)
            new [] { 261.62, -554.913, 0.564 },         // Uranus
            new [] { 107.21, 468.75, 0.662 },           // Neptune
            new [] { 252.66, -56.364, 0.413 }           // Pluto
        };

        #endregion Terms

        /// <summary>
        /// Gets planet semidiameter
        /// </summary>
        /// <param name="p">Planet serial number, starting from 1 (Mercury) to 8 (Neptune).</param>
        /// <param name="distance">Distance to the planet from Earth, in AU.</param>
        /// <returns>
        /// Planet semidiameter, in arcseconds.
        /// </returns>
        public static double Semidiameter(int p, double distance)
        {
            if (p < 1 || p > 8 || p == 3)
                throw new ArgumentException("Planet serial number should be in range from 1 (= Mercury) to 8 (= Neptune), excluding 3 (= Earth).", nameof(p));

            return s0[p - 1] / distance;
        }

        /// <summary>
        /// Calculates parallax of the planet
        /// </summary>
        /// <param name="distance">Distance, in A.U.</param>
        /// <returns>Parallax value in degrees.</returns>
        /// <remarks>
        /// Method is taken from AA(II), p. 279
        /// </remarks>
        public static double Parallax(double distance)
        {
            return 8.794 / distance / 3600;
        }

        /// <summary>
        /// Calculates magnitude of the planet.
        /// </summary>
        /// <param name="planet">Planet number, from 1 (Mercury) to 8 (Neptune).</param>
        /// <param name="Delta">Distance Earth-Planet, in AU.</param>
        /// <param name="r">Distance Sun-Planet, in AU.</param>
        /// <param name="i">Phase angle of the planet, in degrees.</param>
        /// <returns>Returns magnitude value for the planet.</returns>
        /// <remarks>
        /// Method is taken from AA(II), p. 286.
        /// </remarks>
        public static float Magnitude(int planet, double Delta, double r, double i)
        {
            double i2 = i * i;
            double i3 = i2 * i;

            switch (planet)
            {
                case 1:
                    return (float)(-0.42 + 5 * Math.Log10(r * Delta) + 0.0380 * i - 0.000273 * i2 + 0.000002 * i3);
                case 2:
                    return (float)(-4.40 + 5 * Math.Log10(r * Delta) + 0.0009 * i + 0.000239 * i2 - 0.00000065 * i3);
                case 4:
                    return (float)(-1.52 + 5 * Math.Log10(r * Delta) + 0.016 * i);
                case 5:
                    return (float)(-9.40 + 5 * Math.Log10(r * Delta) + 0.005 * i);
                case 6:
                    // N.B.: this value does not take into account magnitude component of rings.
                    return (float)(-8.88 + 5 * Math.Log10(r * Delta));
                case 7:
                    return (float)(-7.19 + 5 * Math.Log10(r * Delta));
                case 8:
                    return (float)(-6.87 + 5 * Math.Log10(r * Delta));
                default:
                    throw new ArgumentException("Wrong planet number.", nameof(planet));
            }
        }

        /// <summary>
        /// Calculates visible appearance of planet for given date.
        /// </summary>
        /// <param name="jd">Julian day</param>
        /// <param name="planet">Planet number to calculate appearance, 1 = Mercury, 2 = Venus and etc.</param>
        /// <param name="eq">Equatorial coordinates of the planet</param>
        /// <param name="distance">Distance from the planet to the Earth</param>
        /// <returns>Appearance parameters of the planet</returns>
        /// <remarks>
        /// This method is based on book "Practical Ephemeris Calculations", Montenbruck.
        /// See topic 6.4, pp. 88-92.
        /// </remarks>
        public static PlanetAppearance PlanetAppearance(double jd, int planet, CrdsEquatorial eq, double distance)
        { 
            PlanetAppearance a = new PlanetAppearance();

            double d = jd - 2451545.0;
            double T = d / 36525.0;

            // coordinates of the point to which the north pole of the planet is pointing.
            CrdsEquatorial eq0 = new CrdsEquatorial();

            eq0.Alpha = Angle.To360(cAlpha0[planet - 1][0] + cAlpha0[planet - 1][1] * T + cAlpha0[planet - 1][2] * T);
            eq0.Delta = cDelta0[planet - 1][0] + cDelta0[planet - 1][1] * T + cDelta0[planet - 1][2] * T;

            // take light time effect into account
            d -= PlanetPositions.LightTimeEffect(distance);
            T = d / 36525.0;

            // position of null meridian
            double W = Angle.To360(cW[planet - 1][0] + cW[planet - 1][1] * d + cW[planet - 1][2] * T);

            double delta = Angle.ToRadians(eq.Delta);
            double delta0 = Angle.ToRadians(eq0.Delta);
            double dAlpha0 = Angle.ToRadians(eq0.Alpha - eq.Alpha);

            double sinD = -Math.Sin(delta0) * Math.Sin(delta) - Math.Cos(delta0) * Math.Cos(delta) * Math.Cos(dAlpha0);

            // planetographic latitude of the Earth
            a.D = Angle.ToDegrees(Math.Asin(sinD));

            double cosD = Math.Cos(Angle.ToRadians(a.D));

            double sinP = Math.Cos(delta0) * Math.Sin(dAlpha0) / cosD;
            double cosP = (Math.Sin(delta0) * Math.Cos(delta) - Math.Cos(delta0) * Math.Sin(delta) * Math.Cos(dAlpha0)) / cosD;

            // position angle of the axis
            a.P = Angle.To360(Angle.ToDegrees(Math.Atan2(sinP, cosP)));

            double sinK = (-Math.Cos(delta0) * Math.Sin(delta) + Math.Sin(delta0) * Math.Cos(delta) * Math.Cos(dAlpha0)) / cosD;

            double cosK = Math.Cos(delta) * Math.Sin(dAlpha0) / cosD;

            double K = Angle.ToDegrees(Math.Atan2(sinK, cosK));

            // planetographic longitude of the central meridian
            a.CM = planet == 5 ? 
                JupiterCM2(jd) : 
                Angle.To360(Math.Sign(W) * (W - K));
            
            return a;
        }

        /// <summary>
        /// Calculates longitude of Central Meridian of Jupiter in System II.
        /// </summary>
        /// <param name="jd">Julian Day</param>
        /// <returns>Longitude of Central Meridian of Jupiter in System II, in degrees.</returns>
        /// <remarks>
        /// This method is based on formula described here: <see href="https://www.projectpluto.com/grs_form.htm"/>
        /// </remarks>
        private static double JupiterCM2(double jd)
        {
            double jup_mean = (jd - 2455636.938) * 360.0 / 4332.89709;
            double eqn_center = 5.55 * Math.Sin(Angle.ToRadians(jup_mean));
            double angle = (jd - 2451870.628) * 360.0 / 398.884 - eqn_center;
            double correction = 11 * Math.Sin(Angle.ToRadians(angle))
                                + 5 * Math.Cos(Angle.ToRadians(angle))
                                - 1.25 * Math.Cos(Angle.ToRadians(jup_mean)) - eqn_center;

            double cm = 181.62 + 870.1869147 * jd + correction;
            return Angle.To360(cm);
        }

        /// <summary>
        /// Calculates appearance of Saturn rings
        /// </summary>
        /// <param name="jd">Julian date to calculate for</param>
        /// <param name="saturn">Heliocentric coordinates of Saturn.</param>
        /// <param name="earth">Heliocentric coordinates of Earth.</param>
        /// <param name="epsilon">True obliquity of ecliptic.</param>
        /// <returns>
        /// Appearance data for Saturn rings.
        /// </returns>
        /// <remarks>
        /// Method is taken from AA(II), chapter 45.
        /// </remarks>
        public static RingsAppearance SaturnRings(double jd, CrdsHeliocentrical saturn, CrdsHeliocentrical earth, double epsilon)
        {
            RingsAppearance rings = new RingsAppearance();
            double T = (jd - 2451545.0) / 36525.0;
            double T2 = T * T;

            double i = 28.075216 - 0.012998 * T + 0.000004 * T2;
            double Omega = 169.508470 + 1.394681 * T + 0.000412 * T2;

            double lambda0 = Omega - 90;
            double beta0 = 90 - i;

            i = Angle.ToRadians(i);
            Omega = Angle.ToRadians(Omega);

            CrdsEcliptical ecl = saturn.ToRectangular(earth).ToEcliptical();

            double beta = Angle.ToRadians(ecl.Beta);
            double lambda = Angle.ToRadians(ecl.Lambda);

            rings.B = Angle.ToDegrees(Math.Asin(Math.Sin(i) * Math.Cos(beta) * Math.Sin(lambda - Omega) - Math.Cos(i) * Math.Sin(beta)));
            rings.a = 375.35 / ecl.Distance;
            rings.b = rings.a * Math.Sin(Math.Abs(Angle.ToRadians(rings.B)));

            double N = 113.6655 + 0.8771 * T;
            double l_ = Angle.ToRadians(saturn.L - 0.01759 / saturn.R);
            double b_ = Angle.ToRadians(saturn.B - 0.000764 * Math.Cos(Angle.ToRadians(saturn.L - N)) / saturn.R);
        
            double U1 = Angle.ToDegrees(Math.Atan((Math.Sin(i) * Math.Sin(b_) + Math.Cos(i) * Math.Cos(b_) * Math.Sin(l_ - Omega)) / (Math.Cos(b_) * Math.Cos(l_ - Omega))));
            double U2 = Angle.ToDegrees(Math.Atan((Math.Sin(i) * Math.Sin(beta) + Math.Cos(i) * Math.Cos(beta) * Math.Sin(lambda - Omega)) / (Math.Cos(beta) * Math.Cos(lambda - Omega))));

            rings.DeltaU = Math.Abs(U1 - U2);

            CrdsEcliptical eclPole = new CrdsEcliptical();
            eclPole.Set(lambda0, beta0);

            CrdsEquatorial eq = ecl.ToEquatorial(epsilon);
            CrdsEquatorial eqPole = eclPole.ToEquatorial(epsilon);

            double alpha = Angle.ToRadians(eq.Alpha);
            double delta = Angle.ToRadians(eq.Delta);
            double alpha0 = Angle.ToRadians(eqPole.Alpha);
            double delta0 = Angle.ToRadians(eqPole.Delta);

            double y = Math.Cos(delta0) * Math.Sin(alpha0 - alpha);
            double x = Math.Sin(delta0) * Math.Cos(delta) - Math.Cos(delta0) * Math.Sin(delta) * Math.Cos(alpha0 - alpha);

            rings.P = Angle.ToDegrees(Math.Atan2(y, x));

            return rings;
        }

        public static double GreatRedSpotLongitude(double jd, GreatRedSpotSettings grs)
        {
            // Based on https://github.com/Stellarium/stellarium/blob/24a28f335f5277374cd387a1eda9ca7c7eaa507e/src/core/modules/Planet.cpp#L1145
            return Angle.To360(grs.Longitude + grs.MonthlyDrift * 12 * (jd - grs.Epoch) / 365.25);
        }
    }

    public class GreatRedSpotSettings
    {
        public double Epoch { get; set; }
        public double MonthlyDrift { get; set; }
        public double Longitude { get; set; }
    }
}
