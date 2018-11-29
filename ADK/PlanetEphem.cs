using System;

namespace ADK
{
    public static class PlanetEphem
    {
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

            double[][] cAlpha0 = new double[][] 
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

            double[][] cDelta0 = new double[][]
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

            // coordinates of the rotation axis 
            CrdsEquatorial eq0 = new CrdsEquatorial();

            eq0.Alpha = Angle.To360(cAlpha0[planet - 1][0] + cAlpha0[planet - 1][1] * T + cAlpha0[planet - 1][2] * T);
            eq0.Delta = Angle.To360(cDelta0[planet - 1][0] + cDelta0[planet - 1][1] * T + cDelta0[planet - 1][2] * T);

            // take light time effect into account
            d -= PlanetPositions.LightTimeEffect(distance);
            T = d / 36525.0;

            // position of the null meridian
            double[][] cW = new double[][]
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

            double W = Angle.To360(cW[planet - 1][0] + cW[planet - 1][1] * d + cW[planet - 1][2] * T);

            double delta = Angle.ToRadians(eq.Delta);
            double alpha = Angle.ToRadians(eq.Alpha);

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
            a.CM = Angle.To360(Math.Sign(W) * (W - K));
            
            return a;
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
        // TODO: optimize conversions
        public static RingsAppearance SaturnRings(double jd, CrdsHeliocentrical saturn, CrdsHeliocentrical earth, double epsilon)
        {
            RingsAppearance rings = new RingsAppearance();
            double T = (jd - 2451545.0) / 36525.0;
            double T2 = T * T;

            double i = 28.075216 - 0.012998 * T + 0.000004 * T2;
            double Omega = 169.508470 + 1.394681 * T + 0.000412 * T2;

            CrdsEcliptical ecl = saturn.ToRectangular(earth).ToEcliptical();

            rings.B = Angle.ToDegrees(Math.Asin(Math.Sin(Angle.ToRadians(i)) * Math.Cos(Angle.ToRadians(ecl.Beta)) * Math.Sin(Angle.ToRadians(ecl.Lambda - Omega)) - Math.Cos(Angle.ToRadians(i)) * Math.Sin(Angle.ToRadians(ecl.Beta))));
            rings.a = 375.35 / ecl.Distance;
            rings.b = rings.a * Math.Sin(Math.Abs(Angle.ToRadians(rings.B)));

            double N = 113.6655 + 0.8771 * T;
            double l_ = saturn.L - 0.01759 / saturn.R;
            double b_ = saturn.B - 0.000764 * Math.Cos(Angle.ToRadians(saturn.L - N)) / saturn.R;

            double U1 = Angle.ToDegrees(Math.Atan((Math.Sin(Angle.ToRadians(i)) * Math.Sin(Angle.ToRadians(b_)) + Math.Cos(Angle.ToRadians(i)) * Math.Cos(Angle.ToRadians(b_)) * Math.Sin(Angle.ToRadians(l_ - Omega))) / (Math.Cos(Angle.ToRadians(b_)) * Math.Cos(Angle.ToRadians(l_ - Omega)))));
            double U2 = Angle.ToDegrees(Math.Atan((Math.Sin(Angle.ToRadians(i)) * Math.Sin(Angle.ToRadians(ecl.Beta)) + Math.Cos(Angle.ToRadians(i)) * Math.Cos(Angle.ToRadians(ecl.Beta)) * Math.Sin(Angle.ToRadians(ecl.Lambda - Omega))) / (Math.Cos(Angle.ToRadians(ecl.Beta)) * Math.Cos(Angle.ToRadians(ecl.Lambda - Omega)))));

            rings.DeltaU = Math.Abs(U1 - U2);

            double lambda0 = Omega - 90;
            double beta0 = 90 - i;

            CrdsEcliptical eclPole = new CrdsEcliptical();
            eclPole.Set(lambda0, beta0);

            CrdsEquatorial eq = ecl.ToEquatorial(epsilon);
            CrdsEquatorial eqPole = eclPole.ToEquatorial(epsilon);

            double y = Math.Cos(Angle.ToRadians(eqPole.Delta)) * Math.Sin(Angle.ToRadians(eqPole.Alpha - eq.Alpha));
            double x = Math.Sin(Angle.ToRadians(eqPole.Delta)) * Math.Cos(Angle.ToRadians(eq.Delta)) - Math.Cos(Angle.ToRadians(eqPole.Delta)) * Math.Sin(Angle.ToRadians(eq.Delta)) * Math.Cos(Angle.ToRadians(eqPole.Alpha - eq.Alpha));

            rings.P = Angle.ToDegrees(Math.Atan2(y, x));

            return rings;
        }
    }
}
