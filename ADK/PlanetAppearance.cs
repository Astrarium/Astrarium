using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK
{
    public static class PlanetAppearance
    {
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
            double i = 28.075216 - 0.012998 * T + 0.000004 * T * T;
            double Omega = 169.508470 + 1.394681 * T + 0.000412 * T * T;

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
