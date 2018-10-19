using System;
using System.Collections.Generic;
using System.Text;

namespace ADK
{
    public static class Coordinates
    {
        /// <summary>
        /// Calculates hour angle for a celestial point by its Right Ascension.
        /// </summary>
        /// <param name="theta0">Sidereal Time at Greenwich, in degrees.</param>
        /// <param name="L">Longitude of the observer, in degrees.</param>
        /// <param name="alpha">Right Ascension for the celestial point, in degrees.</param>
        /// <returns>Returns Hour Angle for the celestial point, in degrees.</returns>
        public static double HourAngle(double theta0, double L, double alpha)
        {
            return theta0 - L - alpha;
        }

        /// <summary>
        /// Converts equatorial coodinates to local horizontal
        /// </summary>
        /// <param name="eq">Pair of equatorial coodinates</param>
        /// <param name="geo">Geographical of the observer</param>
        /// <param name="theta0">Local sidereal time</param>
        /// <remarks>
        /// Implementation is taken from AA(I), formulae 12.5, 12.6.
        /// </remarks>
        public static CrdsHorizontal ToHorizontal(this CrdsEquatorial eq, CrdsGeographical geo, double theta0)
        {
            double H = AstroUtils.ToRadian(HourAngle(theta0, geo.Longitude, eq.Alpha));
            double phi = AstroUtils.ToRadian(geo.Latitude);        
            double delta = AstroUtils.ToRadian(eq.Delta);

            CrdsHorizontal hor = new CrdsHorizontal();

            double Y = Math.Sin(H);
            double X = Math.Cos(H) * Math.Sin(phi) - Math.Tan(delta) * Math.Cos(phi);

            hor.Altitude = AstroUtils.ToDegree(Math.Asin(Math.Sin(phi) * Math.Sin(delta) + Math.Cos(phi) * Math.Cos(delta) * Math.Cos(H)));

            hor.Azimuth = AstroUtils.ToDegree(Math.Atan2(Y, X));
            hor.Azimuth = AstroUtils.To360(hor.Azimuth);

            return hor;
        }
    }
}
