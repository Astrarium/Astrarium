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


        // TODO: not finished yet
        public static RTS RiseTransitSet(CrdsEquatorial[] eq, CrdsGeographical location, double deltaT, double theta0, double h0)
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

            double cosH0 = (Math.Sin(Angle.ToRadians(h0)) - Math.Sin(Angle.ToRadians(location.Latitude)) * Math.Sin(Angle.ToRadians(delta[1]))) /
                (Math.Cos(Angle.ToRadians(location.Latitude)) * Math.Cos(Angle.ToRadians(delta[1])));

            if (Math.Abs(cosH0) >= 1)
            {
                throw new Exception("Circumpolar");
            }

            double H0 = Angle.ToDegrees(Math.Acos(cosH0));

            double[] m = new double[3];

            m[0] = (alpha[1] + location.Longitude - theta0) / 360;
            m[1] = m[0] - H0 / 360;
            m[2] = m[0] + H0 / 360;

            for (int i = 0; i < 3; i++)
            {
                if (m[i] >= 1) m[i] -= 1;
                if (m[i] < 0) m[i] += 1;
            }

            Angle.NormalizeAngles(alpha);
            Angle.NormalizeAngles(delta);

            double[] x = new double[] { 0, 0.5, 1 };

            for (int i = 0; i < 3; i++)
            {
                double deltaM;

                do
                {
                    double theta = Angle.To360(theta0 + 360.985647 * m[i]) - 180;

                    double n = m[i] + deltaT / 86400;

                    double a = Angle.To360(Interpolation.Lagrange(x, alpha, n));
                    double d = Interpolation.Lagrange(x, delta, n);

                    var eq0 = new CrdsEquatorial(a, d);

                    double H = Angle.To360(Coordinates.HourAngle(theta, location.Longitude, a));
                    if (H > 180) H -= 360;


                    var h = eq0.ToHorizontal(location, theta);



                    // transit
                    if (i == 0)
                    {
                        deltaM = -H / 360;
                    }
                    else
                    {
                        deltaM = (h.Altitude - h0) / (360 * Math.Cos(Angle.ToRadians(d)) * Math.Cos(Angle.ToRadians(location.Latitude) * Math.Sin(Angle.ToRadians(H))));
                    }

                    m[i] += deltaM;

                }
                while (Math.Abs(deltaM * 24 * 60) > 1);

            }

            return new RTS()
            {
                Transit = m[0],
                Rise = m[1],
                Set = m[2]
            };
        }

        public static RTS RiseTransitSet2(double jd, double deltaPsi, double epsilon, CrdsEquatorial[] eq, CrdsGeographical location, double deltaT, double h0)
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
                var sidTime = Date.ApparentSiderealTime(jd + n, deltaPsi, epsilon);
                var h = eqP.ToHorizontal(location, sidTime);
                h.Altitude += h0;
                hor.Add(h);
            }

            var result = new RTS();

            int rise = -1;
            int set = -1;
            for (int i = 0; i < 24; i++)
            {
                // rise:
                if (hor[i].Altitude <= 0 && hor[i + 1].Altitude >= 0)
                {
                    rise = i;
                }
                // set:
                else if (hor[i].Altitude >= 0 && hor[i + 1].Altitude <= 0)
                {
                    set = i;
                }
            }


            for (int i=0; i<2;i++)
            {
                int t;
                double time;

                if (i == 0) t = rise;
                else t = set;

                // If occurs:
                if (t != -1)
                {
                    // eq: at the middle of hour
                    CrdsEquatorial eqP = new CrdsEquatorial();
                    eqP.Alpha = Interpolation.Lagrange(x, alpha, (t + 0.5) / 24.0);
                    eqP.Delta = Interpolation.Lagrange(x, delta, (t + 0.5) / 24.0);

                    var sidTime = Date.ApparentSiderealTime(jd + (t + 0.5) / 24.0, deltaPsi, epsilon);
                    var hor0 = eqP.ToHorizontal(location, sidTime);
                    hor0.Altitude += h0;

                    double n = SolveParabola(hor[rise].Altitude, hor0.Altitude, hor[t + 1].Altitude);

                    time = (t + n) / 24.0;

                }
                else
                {
                    time = Double.NegativeInfinity;
                }

                if (i == 0) result.Rise = time;
                else result.Set = time;
            }

            // TRANSIT
            for (int i = 0; i <= 24; i++)
            {
                double n = (i + 0.5) / 24.0;
               
                // eq: at the middle of hour
                CrdsEquatorial eqP = new CrdsEquatorial();
                eqP.Alpha = Interpolation.Lagrange(x, alpha, n);
                eqP.Delta = Interpolation.Lagrange(x, delta, n);

                var sidTime = Date.ApparentSiderealTime(jd + n, deltaPsi, epsilon);
                var hor0 = eqP.ToHorizontal(location, sidTime);


                if (hor0.Altitude > 0)
                {
                    double nn = SolveParabola(Math.Sin(Angle.ToRadians(hor[i].Azimuth)), Math.Sin(Angle.ToRadians(hor0.Azimuth)), Math.Sin(Angle.ToRadians(hor[i + 1].Azimuth)));
                    if (!double.IsNaN(nn))
                    {
                        result.Transit = (i + nn) / 24.0;
                        
                        break;
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

            if (x1 >= 0 && x1 <= 1) return x1;
            if (x2 >= 0 && x2 <= 1) return x2;

            return Double.NaN;
        }
    }
}
