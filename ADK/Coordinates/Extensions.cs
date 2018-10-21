using System;

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
        /// <returns>Pair of equatorial coordinates</returns>
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

        /// <summary>
        /// Converts ecliptical coordinates to equatorial.
        /// </summary>
        /// <param name="ecl">Pair of ecliptical cooordinates.</param>
        /// <param name="epsilon">Obliquity of the ecliptic, in degrees.</param>
        /// <returns>Pair of equatorial coordinates.</returns>
        public static CrdsEquatorial ToEquatorial(this CrdsEcliptical ecl, double epsilon)
        {
            CrdsEquatorial eq = new CrdsEquatorial();

            epsilon = AstroUtils.ToRadian(epsilon);
            double lambda = AstroUtils.ToRadian(ecl.Lambda);
            double beta = AstroUtils.ToRadian(ecl.Beta);

            double Y = Math.Sin(lambda) * Math.Cos(epsilon) - Math.Tan(beta) * Math.Sin(epsilon);
            double X = Math.Cos(lambda);

            eq.Alpha = AstroUtils.To360(AstroUtils.ToDegree(Math.Atan2(Y, X)));
            eq.Delta = AstroUtils.ToDegree(Math.Asin(Math.Sin(beta) * Math.Cos(epsilon) + Math.Cos(beta) * Math.Sin(epsilon) * Math.Sin(lambda)));

            return eq;
        }

        /// <summary>
        /// Converts equatorial coordinates to ecliptical coordinates. 
        /// </summary>
        /// <param name="eq">Pair of equatorial coordinates.</param>
        /// <param name="epsilon">Obliquity of the ecliptic, in degrees.</param>
        /// <returns></returns>
        public static CrdsEcliptical ToEcliptical(this CrdsEquatorial eq, double epsilon)
        {
            CrdsEcliptical ecl = new CrdsEcliptical();

            epsilon = AstroUtils.ToRadian(epsilon);
            double alpha = AstroUtils.ToRadian(eq.Alpha);
            double delta = AstroUtils.ToRadian(eq.Delta);

            double Y = Math.Sin(alpha) * Math.Cos(epsilon) + Math.Tan(delta) * Math.Sin(epsilon);
            double X = Math.Cos(alpha);
            
            ecl.Lambda = AstroUtils.ToDegree(Math.Atan2(Y, X));
            ecl.Beta = AstroUtils.ToDegree(Math.Asin(Math.Sin(delta) * Math.Cos(epsilon) - Math.Cos(delta) * Math.Sin(epsilon) * Math.Sin(alpha)));

            return ecl;
        }

        /// <summary>
        /// Converts equatorial coordinates (for equinox B1950.0) to galactical coordinates. 
        /// </summary>
        /// <param name="eq">Equatorial coordinates for equinox B1950.0</param>
        /// <returns>Galactical coordinates.</returns>
        public static CrdsGalactical ToGalactical(this CrdsEquatorial eq)
        {
            CrdsGalactical gal = new CrdsGalactical();

            double alpha0_alpha = AstroUtils.ToRadian(192.25 - eq.Alpha);
            double delta = AstroUtils.ToRadian(eq.Delta);
            double delta0 = AstroUtils.ToRadian(27.4);
            
            double Y = Math.Sin(alpha0_alpha);
            double X = Math.Cos(alpha0_alpha) * Math.Sin(delta0) - Math.Tan(delta) * Math.Cos(delta0);
            double sinb = Math.Sin(delta) * Math.Sin(delta0) + Math.Cos(delta) * Math.Cos(delta0) * Math.Cos(alpha0_alpha);

            gal.l = AstroUtils.To360(303 - AstroUtils.ToDegree(Math.Atan2(Y, X)));
            gal.b = AstroUtils.ToDegree(Math.Asin(sinb));
            return gal;
        }

        /// <summary>
        /// Converts galactical coodinates to equatorial, for equinox B1950.0. 
        /// </summary>
        /// <param name="gal">Galactical coodinates.</param>
        /// <returns>Equatorial coodinates, for equinox B1950.0.</returns>
        public static CrdsEquatorial ToEquatorial(this CrdsGalactical gal)
        {
            CrdsEquatorial eq = new CrdsEquatorial();

            double l_l0 = AstroUtils.ToRadian(gal.l - 123.0);
            double delta0 = AstroUtils.ToRadian(27.4);
            double b = AstroUtils.ToRadian(gal.b);

            double Y = Math.Sin(l_l0);
            double X = Math.Cos(l_l0) * Math.Sin(delta0) - Math.Tan(b) * Math.Cos(delta0);
            double sinDelta = Math.Sin(b) * Math.Sin(delta0) + Math.Cos(b) * Math.Cos(delta0) * Math.Cos(l_l0);

            eq.Alpha = AstroUtils.To360(AstroUtils.ToDegree(Math.Atan2(Y, X)) + 12.25);
            eq.Delta = AstroUtils.ToDegree(Math.Asin(sinDelta));
            return eq;
        }

        /// <summary>
        /// Converts ecliptical coordinates to rectangular coordinates. 
        /// </summary>
        /// <param name="ecl">Ecliptical coordinates</param>
        /// <param name="epsilon">Obliquity of the ecliptic, in degrees.</param>
        /// <returns>Rectangular coordinates.</returns>
        public static CrdsRectangular ToRectangular(this CrdsEcliptical ecl, double epsilon)
        {
            CrdsRectangular rect = new CrdsRectangular();

            double beta = AstroUtils.ToRadian(ecl.Beta);
            double lambda = AstroUtils.ToRadian(ecl.Lambda);
            double R = ecl.Distance;

            epsilon = AstroUtils.ToRadian(epsilon);

            double cosBeta = Math.Cos(beta);
            double sinBeta = Math.Sin(beta);
            double sinLambda = Math.Sin(lambda);
            double cosLambda = Math.Cos(lambda);
            double sinEpsilon = Math.Sin(epsilon);
            double cosEpsilon = Math.Cos(epsilon);

            rect.X = R * cosBeta * cosLambda;
            rect.Y = R * (cosBeta * sinLambda * cosEpsilon - sinBeta * sinEpsilon);
            rect.Z = R * (cosBeta * sinLambda * sinEpsilon + sinBeta * cosEpsilon);
            return rect;
        }
    }
}
