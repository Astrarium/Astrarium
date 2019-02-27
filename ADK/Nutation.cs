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
        /// Calculates nutation elements for given instant.
        /// </summary>
        /// <param name="jd">Julian Day, corresponding to the given instant.</param>
        /// <returns>Aberration elements for the given instant.</returns>
        /// <remarks>
        /// The method is taken from AA(II), page 144.
        /// Accuracy of the method is 0.1" for Δε and 0.5" for Δψ.
        /// </remarks>
        public static NutationElements NutationElements(double jd)
        {
            double T = (jd - 2451545) / 36525.0;

            // Longitude of the ascending node of Moon's mean orbit on the ecliptic, 
            // measured from the mean equinox of the date: 
            double Omega = 125.04452 - 1934.136261 * T;

            // Mean longutude of Sun
            double L = 280.4665 + 36000.7698 * T;

            // Mean longitude of Moon
            double L_ = 218.3165 + 481267.8813 * T;

            double deltaEpsilon = 9.20 * Math.Cos(Angle.ToRadians(Omega)) + 0.57 * Math.Cos(Angle.ToRadians(2 * L)) + 0.10 * Math.Cos(Angle.ToRadians(2 * L_)) - 0.09 * Math.Cos(Angle.ToRadians(2 * Omega));

            double deltaPsi = -17.20 * Math.Sin(Angle.ToRadians(Omega)) - 1.32 * Math.Sin(Angle.ToRadians(2 * L)) - 0.23 * Math.Sin(Angle.ToRadians(2 * L_)) + 0.21 * Math.Sin(Angle.ToRadians(2 * Omega));

            return new NutationElements()
            {
                deltaEpsilon = deltaEpsilon / 3600,
                deltaPsi = deltaPsi / 3600
            };
        }

        /// <summary>
        /// Returns nutation corrections for ecliptical coordinates.
        /// </summary>
        /// <param name="deltaPsi">Nutation in longitude (Δψ) for given instant, in degrees.</param>
        /// <remarks>See AA(II), page 150, last paragraph.</remarks>
        public static CrdsEcliptical NutationEffect(double deltaPsi)
        {
            return new CrdsEcliptical(deltaPsi, 0);
        }

        /// <summary>
        /// Returns nutation corrections for equatorial coordiantes.
        /// </summary>
        /// <param name="eq">Initial (not corrected) equatorial coordiantes.</param>
        /// <param name="ne">Nutation elements for given instant.</param>
        /// <param name="epsilon">True obliquity of the ecliptic (ε), in degrees.</param>
        /// <returns>Nutation corrections for equatorial coordiantes.</returns>
        /// <remarks>AA(II), formula 23.1</remarks>
        public static CrdsEquatorial NutationEffect(CrdsEquatorial eq, NutationElements ne, double epsilon)
        {
            CrdsEquatorial correction = new CrdsEquatorial();

            epsilon = Angle.ToRadians(epsilon);
            double alpha = Angle.ToRadians(eq.Alpha);
            double delta = Angle.ToRadians(eq.Delta);

            correction.Alpha = (Math.Cos(epsilon) + Math.Sin(epsilon) * Math.Sin(alpha) * Math.Tan(delta)) * ne.deltaPsi - (Math.Cos(alpha) * Math.Tan(delta)) * ne.deltaEpsilon;
            correction.Delta = Math.Sin(epsilon) * Math.Cos(alpha) * ne.deltaPsi + Math.Sin(alpha) * ne.deltaEpsilon;
            return correction;
        }
    }

    /// <summary>
    /// Defines elements needed for calculation of nutation effect.
    /// </summary>
    public class NutationElements
    {
        /// <summary>
        /// Nutation in longitude (Δψ), in degrees.
        /// </summary>
        public double deltaPsi { get; set; }

        /// <summary>
        /// Nutation in obliquity (Δε), in degrees.
        /// </summary>
        public double deltaEpsilon { get; set; }
    }
}
