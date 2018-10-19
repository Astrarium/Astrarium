using System;
using System.Collections.Generic;
using System.Text;

namespace ADK
{
    /// <summary>
    /// Contains extension methods for transformation of coordinates.
    /// </summary>
    public static class Coordinates
    {
        /// <summary>
        /// Calculates the local hour angle for a celestial point by its Right Ascension.
        /// Measured westwards from the South. 
        /// </summary>
        /// <param name="theta0">Sidereal Time at Greenwich, in degrees.</param>
        /// <param name="L">Longitude of the observer, in degrees.</param>
        /// <param name="alpha">Right Ascension for the celestial point, in degrees.</param>
        /// <returns>Returns local hour angle for the celestial point, in degrees.</returns>
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

        /// <summary>
        /// Converts local horizontal coordinates to equatorial coordinates. 
        /// </summary>
        /// <param name="hor">Pair of local horizontal coordinates.</param>
        /// <param name="geo">Geographical of the observer</param>
        /// <param name="theta0">Local sidereal time.</param>
        /// <returns>Paire of equatorial coordinates</returns>
        public static CrdsEquatorial ToEquatorial(this CrdsHorizontal hor, CrdsGeographical geo, double theta0)
        {
            CrdsEquatorial eq = new CrdsEquatorial();
            double A = AstroUtils.ToRadian(hor.Azimuth);
            double h = AstroUtils.ToRadian(hor.Altitude);
            double phi = AstroUtils.ToRadian(geo.Latitude);

            double Y = Math.Sin(A);
            double X = Math.Cos(A) * Math.Sin(phi) + Math.Tan(h) * Math.Cos(phi);

            double H = AstroUtils.ToDegree(Math.Atan2(Y, X));

            eq.Alpha = AstroUtils.To360(theta0 - geo.Longitude - H);
            eq.Delta = AstroUtils.ToDegree(Math.Asin(Math.Sin(phi) * Math.Sin(h) - Math.Cos(phi) * Math.Cos(h) * Math.Cos(A)));

            return eq;
        }
    }
}
