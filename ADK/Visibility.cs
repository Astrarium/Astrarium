using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK
{
    /// <summary>
    /// Contains methods for calculating visibility details of celestial bodies
    /// </summary>
    public static class Visibility
    {
        /// <summary>
        /// Calculates instants of rising, transit and setting for non-stationary celestial body for the desired date.
        /// Non-stationary in this particular case means that body has fastly changing celestial coordinates during the day.
        /// </summary>
        /// <param name="eq">Array of three equatorial coordinates of the celestial body correspoding to local midnight, local noon, and local midnight of the following day after the desired date respectively.</param>
        /// <param name="location">Geographical location of the observation point.</param>
        /// <param name="theta0">Apparent sidereal time at Greenwich for local midnight of the desired date.</param>
        /// <param name="pi">Horizontal equatorial parallax of the body.</param>
        /// <param name="sd">Visible semidiameter of the body, expressed in degrees.</param>
        /// <returns>Instants of rising, transit and setting for the celestial body for the desired date.</returns>
        public static RTS RiseTransitSet(CrdsEquatorial[] eq, CrdsGeographical location, double theta0, double pi = 0, double sd = 0)
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

            Angle.Align(alpha);
            Angle.Align(delta);

            List<CrdsHorizontal> hor = new List<CrdsHorizontal>();
            for (int i = 0; i <= 24; i++)
            {
                double n = i / 24.0;
                CrdsEquatorial eq0 = InterpolateEq(alpha, delta, n);
                var sidTime = InterpolateSiderialTime(theta0, n);
                hor.Add(eq0.ToTopocentric(location, sidTime, pi).ToHorizontal(location, sidTime));
            }

            var result = new RTS();

            for (int i = 0; i < 24; i++)
            {
                double n = (i + 0.5) / 24.0;

                CrdsEquatorial eq0 = InterpolateEq(alpha, delta, n);

                var sidTime = InterpolateSiderialTime(theta0, n);
                var hor0 = eq0.ToTopocentric(location, sidTime, pi).ToHorizontal(location, sidTime);

                if (double.IsNaN(result.Transit) && hor0.Altitude > 0)
                {
                    double r = SolveParabola(Math.Sin(Angle.ToRadians(hor[i].Azimuth)), Math.Sin(Angle.ToRadians(hor0.Azimuth)), Math.Sin(Angle.ToRadians(hor[i + 1].Azimuth)));
                    if (!double.IsNaN(r))
                    {
                        double t = (i + r) / 24.0;

                        eq0 = InterpolateEq(alpha, delta, t);
                        sidTime = InterpolateSiderialTime(theta0, t);

                        result.Transit = t;
                        result.TransitAltitude = eq0.ToTopocentric(location, sidTime, pi).ToHorizontal(location, sidTime).Altitude;
                    }
                }

                if (double.IsNaN(result.Rise) || double.IsNaN(result.Set))
                {
                    double r = SolveParabola(hor[i].Altitude + sd, hor0.Altitude + sd, hor[i + 1].Altitude + sd);

                    if (!double.IsNaN(r))
                    {
                        double t = (i + r) / 24.0;
                        eq0 = InterpolateEq(alpha, delta, t);
                        sidTime = InterpolateSiderialTime(theta0, t);

                        if (double.IsNaN(result.Rise) && hor[i].Altitude + sd < 0 && hor[i + 1].Altitude + sd > 0)
                        {
                            result.Rise = t;
                            result.RiseAzimuth = eq0.ToTopocentric(location, sidTime, pi).ToHorizontal(location, sidTime).Azimuth;
                        }

                        if (double.IsNaN(result.Set) && hor[i].Altitude + sd > 0 && hor[i + 1].Altitude + sd < 0)
                        {
                            result.Set = t;
                            result.SetAzimuth = eq0.ToTopocentric(location, sidTime, pi).ToHorizontal(location, sidTime).Azimuth;
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

        /// <summary>
        /// Calculates instants of rising, transit and setting for stationary celestial body for the desired date.
        /// Stationary in this particular case means that body has unchanged (or slightly changing) celestial coordinates during the day.
        /// </summary>
        /// <param name="eq">Equatorial coordinates of the celestial body.</param>
        /// <param name="location">Geographical location of the observation point.</param>
        /// <param name="theta0">Apparent sidereal time at Greenwich for local midnight of the desired date.</param>
        /// <returns>Instants of rising, transit and setting for the celestial body for the desired date.</returns>
        public static RTS RiseTransitSet(CrdsEquatorial eq, CrdsGeographical location, double theta0, double sd = 0)
        {
            List<CrdsHorizontal> hor = new List<CrdsHorizontal>();
            for (int i = 0; i <= 24; i++)
            {
                double n = i / 24.0;
                var sidTime = InterpolateSiderialTime(theta0, n);
                hor.Add(eq.ToHorizontal(location, sidTime));
            }

            var result = new RTS();

            for (int i = 0; i < 24; i++)
            {
                double n = (i + 0.5) / 24.0;

                var sidTime = InterpolateSiderialTime(theta0, n);
                var hor0 = eq.ToHorizontal(location, sidTime);

                if (double.IsNaN(result.Transit) && hor0.Altitude > 0)
                {
                    double r = SolveParabola(Math.Sin(Angle.ToRadians(hor[i].Azimuth)), Math.Sin(Angle.ToRadians(hor0.Azimuth)), Math.Sin(Angle.ToRadians(hor[i + 1].Azimuth)));
                    if (!double.IsNaN(r))
                    {
                        double t = (i + r) / 24.0;                        
                        sidTime = InterpolateSiderialTime(theta0, t);

                        result.Transit = t;
                        result.TransitAltitude = eq.ToHorizontal(location, sidTime).Altitude;
                    }
                }

                if (double.IsNaN(result.Rise) || double.IsNaN(result.Set))
                {
                    double r = SolveParabola(hor[i].Altitude + sd, hor0.Altitude + sd, hor[i + 1].Altitude + sd);

                    if (!double.IsNaN(r))
                    {
                        double t = (i + r) / 24.0;
                        sidTime = InterpolateSiderialTime(theta0, t);

                        if (double.IsNaN(result.Rise) && hor[i].Altitude + sd < 0 && hor[i + 1].Altitude + sd > 0)
                        {
                            result.Rise = t;
                            result.RiseAzimuth = eq.ToHorizontal(location, sidTime).Azimuth;
                        }

                        if (double.IsNaN(result.Set) && hor[i].Altitude + sd > 0 && hor[i + 1].Altitude + sd < 0)
                        {
                            result.Set = t;
                            result.SetAzimuth = eq.ToHorizontal(location, sidTime).Azimuth;
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

        private static double InterpolateSiderialTime(double theta0, double n)
        {
            return Angle.To360(theta0 + n * 360.98564736629);
        }

        private static CrdsEquatorial InterpolateEq(double[] alpha, double[] delta, double n)
        {
            double[] x = new double[] { 0, 0.5, 1 };
            CrdsEquatorial eq = new CrdsEquatorial();
            eq.Alpha = Interpolation.Lagrange(x, alpha, n);
            eq.Delta = Interpolation.Lagrange(x, delta, n);
            return eq;
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
