using System;

namespace ADK
{
    /// <summary>
    /// Contains methods for calculation of aberration effects.
    /// </summary>
    public static class Aberration
    {
        /// <summary>
        /// Constant of aberration
        /// </summary>
        private const double k = 20.49552;

        /// <summary>
        /// Calculates the aberration effect for the Sun.
        /// </summary>
        /// <param name="distance">Distance Sun-Earth, in astronomical units.</param>
        /// <returns>Returns aberration correction for ecliptical coordinates.</returns>
        public static CrdsEcliptical AberrationEffect(double distance)
        {
            return new CrdsEcliptical(-20.4898 / distance / 3600, 0);
        }

        /// <summary>
        /// Calculates aberration elements for given instant.
        /// </summary>
        /// <param name="jde">Julian Ephemeris Day, corresponding to the given instant.</param>
        /// <returns>Aberration elements for the given instant.</returns>
        /// <remarks>
        /// AA(II), pp. 151, 163, 164
        /// </remarks>
        public static AberrationElements AberrationElements(double jde)
        {
            double T = (jde - 2451545.0) / 36525.0;
            double T2 = T * T;

            double e = 0.016708634 - 0.000042037 * T - 0.0000001267 * T2;
            double pi = 102.93735 + 1.71946 * T + 0.00046 * T2;

            // geometric true longitude of the Sun
            double L0 = 280.46646 + 36000.76983 * T + 0.0003032 * T2;

            // mean anomaly of the Sun
            double M = 357.52911 + 35999.05029 * T - 0.0001537 * T2;
            M = Angle.ToRadians(M);

            // Sun's equation of the center
            double C = (1.914602 - 0.004817 * T - 0.000014 * T2) * Math.Sin(M)
                + (0.019993 - 0.000101 * T) * Math.Sin(2 * M)
                + 0.000289 * Math.Sin(3 * M);

            return new AberrationElements()
            {
                e = e,
                pi = pi,
                lambda = Angle.To360(L0 + C)
            };
        }

        /// <summary>
        /// Calculates the aberration effect for a celestial body (star or planet) for given instant.
        /// </summary>
        /// <param name="ecl">Ecliptical coordinates of the body (not corrected).</param>
        /// <param name="ae">Aberration elements needed for calculation of aberration correction.</param>
        /// <returns>Returns aberration correction values for ecliptical coordinates.</returns>
        /// <remarks>
        /// AA(II), formula 23.2
        /// </remarks>
        public static CrdsEcliptical AberrationEffect(CrdsEcliptical ecl, AberrationElements ae)
        {
            double thetaLambda = Angle.ToRadians(ae.lambda - ecl.Lambda);
            double piLambda = Angle.ToRadians(ae.pi - ecl.Lambda);
            double beta = Angle.ToRadians(ecl.Beta);

            double dLambda = (-k * Math.Cos(thetaLambda) + ae.e * k * Math.Cos(piLambda)) / Math.Cos(beta);
            double dBeta = -k * Math.Sin(beta) * (Math.Sin(thetaLambda) - ae.e * Math.Sin(piLambda));

            return new CrdsEcliptical(dLambda / 3600, dBeta / 3600);
        }

        /// <summary>
        /// Calculates the aberration effect for a celestial body (star or planet) for given instant.
        /// </summary>
        /// <param name="eq">Equatorial coordinates of the body (not corrected).</param>
        /// <param name="ae">Aberration elements needed for calculation of aberration correction.</param>
        /// <returns>Returns aberration correction values for equatorial coordinates.</returns>
        /// <remarks>AA(II), formula 23.3</remarks>
        public static CrdsEquatorial AberrationEffect(CrdsEquatorial eq, AberrationElements ae, double epsilon)
        {
            double a = Angle.ToRadians(eq.Alpha);
            double d = Angle.ToRadians(eq.Delta);
            double theta = Angle.ToRadians(ae.lambda);
            double pi = Angle.ToRadians(ae.pi);
            epsilon = Angle.ToRadians(epsilon);

            double da = -k * (Math.Cos(a) * Math.Cos(theta) * Math.Cos(epsilon) + Math.Sin(a) * Math.Sin(theta)) / Math.Cos(d)
                + epsilon * k * (Math.Cos(a) * Math.Cos(pi) * Math.Cos(epsilon) + Math.Sin(a) * Math.Sin(pi)) / Math.Cos(d);

            double m = Math.Tan(epsilon) * Math.Cos(d) - Math.Sin(a) * Math.Sin(d);

            double dd = -k * (Math.Cos(theta) * Math.Cos(epsilon) * m
                + Math.Cos(a) * Math.Sin(d) * Math.Sin(theta))
                + epsilon * k * (Math.Cos(pi) * Math.Cos(epsilon) * m + Math.Cos(a) * Math.Sin(d) * Math.Sin(pi));

            return new CrdsEquatorial(da / 3600, dd / 3600);
        }
    }

    /// <summary>
    /// Defines elements needed for calculation of aberration effect.
    /// </summary>
    public class AberrationElements
    {
        /// <summary>
        /// Earth orbit longitude of perihelion
        /// </summary>
        public double pi { get; set; }

        /// <summary>
        /// Eccentricity of the Earth orbit
        /// </summary>
        public double e { get; set; }

        /// <summary>
        /// Sun's true longitude
        /// </summary>
        public double lambda { get; set; }
    }
}
