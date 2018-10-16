using System;
using System.Collections.Generic;
using System.Text;

namespace ADK
{
    /// <summary>
    /// Contains methods for calculation of precessional effects.
    /// </summary>
    public static class Precession
    {
        /// <summary>
        /// Gets precessional elements to convert equatorial coordinates of a point from one epoch to another.
        /// Equatorial coordinates of a point must be reffered to system FK4.
        /// </summary>
        /// <param name="jd0">Initial epoch, in Julian Days.</param>
        /// <param name="jd">Target (final) epoch, in Julian Days.</param>
        /// <returns>
        /// <see cref="PrecessionalElements"/> to convert equatorial coordinates of a point from 
        /// one epoch (<paramref name="jd0"/>) to another (<paramref name="jd"/>).
        /// </returns>
        /// <remarks>
        /// This method is taken from AA(I), chapter 20 ("Precession", topic "The old precessional elements").
        /// </remarks>
        public static PrecessionalElements ElementsFK4(double jd0, double jd)
        {
            PrecessionalElements p = new PrecessionalElements();
            p.InitialEpoch = jd0;
            p.TargetEpoch = jd;

            double T = (jd0 - 2415020.3135) / 36524.2199;
            double t = (jd - jd0) / 36524.2199;

            double t2 = t * t;
            double t3 = t2 * t;

            // all values in seconds of arc
            p.zeta = (2304.250 + 1.396 * T) * t + 0.302 * t2 + 0.018 * t3;
            p.z = p.zeta + 0.791 * t2 + 0.001 * t3;
            p.theta = (2004.682 - 0853 * T) * t - 0.426 * t2 - 0.042 * t3;

            // convert to degress
            p.zeta /= 3600;
            p.z /= 3600;
            p.theta /= 3600;

            return p;
        }

        /// <summary>
        /// Gets precessional elements to convert equatorial coordinates of a point from one epoch to another.
        /// Equatorial coordinates of a point must be reffered to system FK5.
        /// </summary>
        /// <param name="jd0">Initial epoch, in Julian Days.</param>
        /// <param name="jd">Target (final) epoch, in Julian Days.</param>
        /// <returns>
        /// <see cref="PrecessionalElements"/> to convert equatorial coordinates of a point from 
        /// one epoch (<paramref name="jd0"/>) to another (<paramref name="jd"/>).
        /// </returns>
        /// <remarks>
        /// This method is taken from AA(I), chapter 20 ("Precession", topic "Rigorous method").
        /// </remarks>
        public static PrecessionalElements ElementsFK5(double jd0, double jd)
        {
            PrecessionalElements p = new PrecessionalElements();
            p.InitialEpoch = jd0;
            p.TargetEpoch = jd;

            double T = (jd0 - 2451545.0) / 36525.0;
            double t = (jd - jd0) / 36525.0;

            double T2 = T * T;
            double t2 = t * t;
            double t3 = t2 * t;

            // all values in seconds of arc
            p.zeta = (2306.2181 + 1.39656 * T - 0.000139 * T2) * t 
                + (0.30188 - 0.000344 * T) * t2 + 0.017998 * t3;
            p.z = (2306.2181 + 1.39656 * T - 0.000139 * T2) * t
                + (1.09468 + 0.000066 * T) * t2 + 0.018203 * t3;
            p.theta = (2004.3109 - 0.85330 * T - 0.000217 * T2) * t
                 - (0.42665 + 0.000217 * T) * t2 - 0.041833 * t3;

            // convert to degress
            p.zeta /= 3600;
            p.z /= 3600;
            p.theta /= 3600;

            return p;
        }

        /// <summary>
        /// Performs reduction of equatorial coordinates from one epoch to another
        /// with using of precessional elements.
        /// </summary>
        /// <param name="eq0">Equatorial coordinates for initial epoch.</param>
        /// <param name="p">Precessional elements for reduction from initial epoch to target (final) epoch.</param>
        /// <returns>Equatorial coordinates for target (final) epoch.</returns>
        /// <remarks>
        /// This method is taken from AA(I), formula 20.4.
        /// </remarks>
        public static CrdsEquatorial GetEquatorialCoordinates(CrdsEquatorial eq0, PrecessionalElements p)
        {
            CrdsEquatorial eq = new CrdsEquatorial();

            double sinDelta0 = Math.Sin(AstroUtils.ToRadian(eq0.Delta));
            double cosDelta0 = Math.Cos(AstroUtils.ToRadian(eq0.Delta));
            double sinTheta = Math.Sin(AstroUtils.ToRadian(p.theta));
            double cosTheta = Math.Cos(AstroUtils.ToRadian(p.theta));
            double sinAlpha0Zeta = Math.Sin(AstroUtils.ToRadian(eq0.Alpha + p.zeta));
            double cosAlpha0Zeta = Math.Cos(AstroUtils.ToRadian(eq0.Alpha + p.zeta));

            double A = cosDelta0 * sinAlpha0Zeta;
            double B = cosTheta * cosDelta0 * cosAlpha0Zeta - sinTheta * sinDelta0;
            double C = sinTheta * cosDelta0 * cosAlpha0Zeta + cosTheta * sinDelta0;

            eq.Alpha = AstroUtils.ToDegree(Math.Atan2(A, B)) + p.z;
            eq.Alpha = AstroUtils.To360(eq.Alpha);

            if (Math.Abs(C) == 1)
            {
                eq.Delta = AstroUtils.ToDegree(Math.Acos(A * A + B * B));
            }
            else
            {
                eq.Delta = AstroUtils.ToDegree(Math.Asin(C));
            }

            return eq;
        }
    }
}
