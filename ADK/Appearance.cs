using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK
{
    /// <summary>
    /// Contains methods for calculating appearance parameters of celestial bodies
    /// </summary>
    public static class Appearance
    {
        /// <summary>
        /// Gets geocentric elongation angle of the celestial body
        /// </summary>
        /// <param name="sun">Ecliptical geocentrical coordinates of the Sun</param>
        /// <param name="body">Ecliptical geocentrical coordinates of the body</param>
        /// <returns>Geocentric elongation angle, in degrees, from -180 to 180.
        /// Negative sign means western elongation, positive eastern.
        /// </returns>
        /// <remarks>
        /// AA(II), formula 48.2
        /// </remarks>
        // TODO: tests
        public static double Elongation(CrdsEcliptical sun, CrdsEcliptical body)
        {
            double beta = Angle.ToRadians(body.Beta);
            double lambda = Angle.ToRadians(body.Lambda);
            double lambda0 = Angle.ToRadians(sun.Lambda);

            double s = sun.Lambda;
            double b = body.Lambda;

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
        /// Calculates phase angle of celestial body
        /// </summary>
        /// <param name="psi">Geocentric elongation of the body.</param>
        /// <param name="R">Distance Earth-Sun, in any units</param>
        /// <param name="Delta">Distance Earth-body, in the same units</param>
        /// <returns>Phase angle, in degrees, from 0 to 180</returns>
        /// <remarks>
        /// AA(II), formula 48.3.
        /// </remarks>
        /// TODO: tests
        public static double PhaseAngle(double psi, double R, double Delta)
        {
            psi = Angle.ToRadians(Math.Abs(psi));
            double phaseAngle = Angle.ToDegrees(Math.Atan(R * Math.Sin(psi) / (Delta - R * Math.Cos(psi))));
            if (phaseAngle < 0) phaseAngle += 180;
            return phaseAngle;
        }

        /// <summary>
        /// Gets phase value (illuminated fraction of the disk).
        /// </summary>
        /// <param name="phaseAngle">Phase angle of celestial body, in degrees.</param>
        /// <returns>Illuminated fraction of the disk, from 0 to 1.</returns>
        /// <remarks>
        /// AA(II), formula 48.1
        /// </remarks>
        // TODO: tests
        public static double Phase(double phaseAngle)
        {
            return (1 + Math.Cos(Angle.ToRadians(phaseAngle))) / 2;
        }

        // TODO: tests
        public static RTS RiseTransitSet(CrdsEquatorial[] eq, CrdsGeographical location, double theta0, double pi, double h0 = 0)
        {
            if (eq.Length != 3)
                throw new ArgumentException("Number of equatorial coordinates in the array should be equal to 3.");

            double[] alpha = new double[3];
            double[] delta = new double[3];
            for (int i = 0; i < 3; i++)
            {
                alpha[i] = eq[i].Alpha;
                delta[i] = eq[i].Delta;
            }

            Angle.NormalizeAngles(alpha);
            Angle.NormalizeAngles(delta);

            double[] x = new double[] { 0, 0.5, 1 };

            List<CrdsHorizontal> hor = new List<CrdsHorizontal>();
            for (int i = 0; i <= 24; i++)
            {
                double n = i / 24.0;
                CrdsEquatorial eqP = new CrdsEquatorial();
                eqP.Alpha = Interpolation.Lagrange(x, alpha, n);
                eqP.Delta = Interpolation.Lagrange(x, delta, n);
                var sidTime = Angle.To360(theta0 + n * 360.98564736629);
                hor.Add(eqP.ToTopocentric(location, sidTime, pi).ToHorizontal(location, sidTime));
            }

            var result = new RTS();

            for (int i = 0; i < 24; i++)
            {
                double n = (i + 0.5) / 24.0;

                // eqM: at the middle of hour
                CrdsEquatorial eqM = new CrdsEquatorial();
                eqM.Alpha = Interpolation.Lagrange(x, alpha, n);
                eqM.Delta = Interpolation.Lagrange(x, delta, n);

                var sidTime = Angle.To360(theta0 + n * 360.98564736629);
                var hor0 = eqM.ToTopocentric(location, sidTime, pi).ToHorizontal(location, sidTime);

                if (double.IsNaN(result.Transit) && hor0.Altitude > 0)
                {
                    double r = SolveParabola(Math.Sin(Angle.ToRadians(hor[i].Azimuth)), Math.Sin(Angle.ToRadians(hor0.Azimuth)), Math.Sin(Angle.ToRadians(hor[i + 1].Azimuth)));
                    if (!double.IsNaN(r))
                    {
                        result.Transit = (i + r) / 24.0;
                    }
                }

                if (double.IsNaN(result.Rise) || double.IsNaN(result.Set))
                {
                    double r = SolveParabola(hor[i].Altitude + h0, hor0.Altitude + h0, hor[i + 1].Altitude + h0);

                    if (!double.IsNaN(r))
                    {
                        double t = (i + r) / 24.0;

                        if (double.IsNaN(result.Rise) && hor[i].Altitude + h0 < 0 && hor[i + 1].Altitude + h0 > 0)
                        {
                            result.Rise = t;
                        }

                        if (double.IsNaN(result.Set) && hor[i].Altitude + h0 > 0 && hor[i + 1].Altitude + h0 < 0)
                        {
                            result.Set = t;
                        }

                        if (!double.IsNaN(result.Transit) && !double.IsNaN(result.Rise) && !double.IsNaN(result.Set))
                        {
                            break;
                        }
                    }
                }                
            }
       
            return result;
        }

        private static double SolveParabola(double y1, double y2, double y3)
        {
            double a = 2 * y1 - 4 * y2 + 2 * y3;
            double b = -3 * y1 + 4 * y2 - y3;
            double c = y1;

            double D = Math.Sqrt(b * b - 4 * a * c);

            double x1 = (-b - D) / (2 * a);
            double x2 = (-b + D) / (2 * a);

            if (x1 >= 0 && x1 < 1) return x1;
            if (x2 >= 0 && x2 < 1) return x2;

            return double.NaN;
        }
    }
}
