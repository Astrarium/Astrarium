using System;
using System.Collections.Generic;
using System.Text;

namespace ADK
{
    public static class Precession
    {
        public static PrecessionalElements GetPrecessionalElementsFK4(double jd0, double jd)
        {
            PrecessionalElements p = new PrecessionalElements();

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

        public static PrecessionalElements GetPrecessionalElementsFK5(double JDE0, double JDE)
        {
            PrecessionalElements p = new PrecessionalElements();

            double T = (JDE0 - 2451545.0) / 36525.0;
            double t = (JDE - JDE0) / 36525.0;

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

        public static CrdsEquatorial GetEquatorialCoordinatesOfEpoch(CrdsEquatorial eq0, PrecessionalElements p)
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
