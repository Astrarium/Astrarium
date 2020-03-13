using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Algorithms
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
        /// <param name="minAltitude">Minimal altitude of the body above the horizon, in degrees, to detect rise/set. Used only for calculating visibility conditions.</param>
        /// <returns>Instants of rising, transit and setting for the celestial body for the desired date.</returns>
        public static RTS RiseTransitSet(CrdsEquatorial eq, CrdsGeographical location, double theta0, double minAltitude = 0)
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
                    double r = SolveParabola(hor[i].Altitude - minAltitude, hor0.Altitude - minAltitude, hor[i + 1].Altitude - minAltitude);

                    if (!double.IsNaN(r))
                    {
                        double t = (i + r) / 24.0;
                        sidTime = InterpolateSiderialTime(theta0, t);

                        if (double.IsNaN(result.Rise) && hor[i].Altitude - minAltitude < 0 && hor[i + 1].Altitude - minAltitude > 0)
                        {
                            result.Rise = t;
                            result.RiseAzimuth = eq.ToHorizontal(location, sidTime).Azimuth;
                        }

                        if (double.IsNaN(result.Set) && hor[i].Altitude - minAltitude > 0 && hor[i + 1].Altitude - minAltitude < 0)
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

        /// <summary>
        /// Calculates visibity details for the celestial body,
        /// </summary>
        /// <param name="eqBody">Mean equatorial coordinates of the body for the desired day.</param>
        /// <param name="eqSun">Mean equatorial coordinates of the Sun for the desired day.</param>
        /// <param name="minAltitude">Minimal altitude of the body, in degrees, to be considered as approproate for observations. By default it's 5 degrees for planet.</param>
        /// <returns><see cref="VisibilityDetails"/> instance describing details of visibility.</returns>
        // TODO: tests
        public static VisibilityDetails Details(CrdsEquatorial eqBody, CrdsEquatorial eqSun, CrdsGeographical location, double theta0, double minAltitude = 5)
        {
            var details = new VisibilityDetails();

            // period when the planet is above the horizon and its altitude is larger than "minAltitude"
            RTS body = RiseTransitSet(eqBody, location, theta0, minAltitude);

            // period when the Sun is above the horizon
            RTS sun = RiseTransitSet(eqSun, location, theta0);

            // body reaches minimal altitude but Sun does not rise at all (polar night)
            if (body.TransitAltitude > minAltitude && sun.TransitAltitude <= 0)
            {
                details.Period = VisibilityPeriod.WholeNight;
                details.Duration = body.Duration * 24;
            }
            // body does not reach the minimal altitude during the day
            else if (body.TransitAltitude <= minAltitude)
            {
                details.Period = VisibilityPeriod.Invisible;
                details.Duration = 0;
            }
            // there is a day/night change during the day and body reaches minimal altitude
            else if (body.TransitAltitude > minAltitude)
            {
                // "Sun is below horizon" time range, expressed in degrees (0 is midnight, 180 is noon)
                var r1 = new AngleRange(sun.Set * 360, (1 - sun.Duration) * 360);

                // "body is above horizon" time range, expressed in degrees (0 is midnight, 180 is noon)
                var r2 = new AngleRange(body.Rise * 360, body.Duration * 360);

                // find the intersections of two ranges
                var ranges = r1.Overlaps(r2);

                // no intersections of time ranges
                if (!ranges.Any())
                {
                    details.Period = VisibilityPeriod.Invisible;
                    details.Duration = 0;
                    details.Begin = double.NaN;
                    details.End = double.NaN;
                }
                // the body is observable during the day
                else
                {
                    // duration of visibility
                    details.Duration = ranges.Sum(i => i.Range / 360 * 24);

                    // beginning of visibility
                    details.Begin = ranges.First().Start / 360;

                    // end of visibility
                    details.End = (details.Begin + details.Duration / 24) % 1;

                    // Evening time range, expressed in degrees
                    // Start is a sunset time, range is a timespan from sunset to midnight.
                    var rE = new AngleRange(sun.Set * 360, (1 - sun.Set) * 360);

                    // Night time range, expressed in degrees
                    // Start is a midnight time, range is a half of timespan from midnight to sunrise
                    var rN = new AngleRange(0, sun.Rise / 2 * 360);

                    // Morning time range, expressed in degrees
                    // Start is a half of time from midnight to sunrise, range is a time to sunrise
                    var rM = new AngleRange(sun.Rise / 2 * 360, sun.Rise / 2 * 360);

                    foreach (var r in ranges)
                    {
                        var isEvening = r.Overlaps(rE);
                        if (isEvening.Any())
                        {
                            details.Period |= VisibilityPeriod.Evening;
                        }

                        var isNight = r.Overlaps(rN);
                        if (isNight.Any())
                        {
                            details.Period |= VisibilityPeriod.Night;
                        }

                        var isMorning = r.Overlaps(rM);
                        if (isMorning.Any())
                        {
                            details.Period |= VisibilityPeriod.Morning;
                        }
                    }
                }
            }

            return details;
        }

        // TODO: description
        private static double InterpolateSiderialTime(double theta0, double n)
        {
            return Angle.To360(theta0 + n * 360.98564736629);
        }

        // TODO: description
        private static CrdsEquatorial InterpolateEq(double[] alpha, double[] delta, double n)
        {
            double[] x = new double[] { 0, 0.5, 1 };
            CrdsEquatorial eq = new CrdsEquatorial();
            eq.Alpha = Interpolation.Lagrange(x, alpha, n);
            eq.Delta = Interpolation.Lagrange(x, delta, n);
            return eq;
        }

        // TODO: description
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
