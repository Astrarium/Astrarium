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
        /// <param name="geo">Geographical coordinates of the observer</param>
        /// <param name="theta0">Local sidereal time</param>
        /// <remarks>
        /// Implementation is taken from AA(I), formulae 12.5, 12.6.
        /// </remarks>
        public static CrdsHorizontal ToHorizontal(this CrdsEquatorial eq, CrdsGeographical geo, double theta0)
        {
            double H = Angle.ToRadians(HourAngle(theta0, geo.Longitude, eq.Alpha));
            double phi = Angle.ToRadians(geo.Latitude);        
            double delta = Angle.ToRadians(eq.Delta);

            CrdsHorizontal hor = new CrdsHorizontal();

            double Y = Math.Sin(H);
            double X = Math.Cos(H) * Math.Sin(phi) - Math.Tan(delta) * Math.Cos(phi);

            hor.Altitude = Angle.ToDegrees(Math.Asin(Math.Sin(phi) * Math.Sin(delta) + Math.Cos(phi) * Math.Cos(delta) * Math.Cos(H)));

            hor.Azimuth = Angle.ToDegrees(Math.Atan2(Y, X));
            hor.Azimuth = Angle.To360(hor.Azimuth);

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
            double A = Angle.ToRadians(hor.Azimuth);
            double h = Angle.ToRadians(hor.Altitude);
            double phi = Angle.ToRadians(geo.Latitude);

            double Y = Math.Sin(A);
            double X = Math.Cos(A) * Math.Sin(phi) + Math.Tan(h) * Math.Cos(phi);

            double H = Angle.ToDegrees(Math.Atan2(Y, X));

            eq.Alpha = Angle.To360(theta0 - geo.Longitude - H);
            eq.Delta = Angle.ToDegrees(Math.Asin(Math.Sin(phi) * Math.Sin(h) - Math.Cos(phi) * Math.Cos(h) * Math.Cos(A)));

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

            epsilon = Angle.ToRadians(epsilon);
            double lambda = Angle.ToRadians(ecl.Lambda);
            double beta = Angle.ToRadians(ecl.Beta);

            double Y = Math.Sin(lambda) * Math.Cos(epsilon) - Math.Tan(beta) * Math.Sin(epsilon);
            double X = Math.Cos(lambda);

            eq.Alpha = Angle.To360(Angle.ToDegrees(Math.Atan2(Y, X)));
            eq.Delta = Angle.ToDegrees(Math.Asin(Math.Sin(beta) * Math.Cos(epsilon) + Math.Cos(beta) * Math.Sin(epsilon) * Math.Sin(lambda)));

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

            epsilon = Angle.ToRadians(epsilon);
            double alpha = Angle.ToRadians(eq.Alpha);
            double delta = Angle.ToRadians(eq.Delta);

            double Y = Math.Sin(alpha) * Math.Cos(epsilon) + Math.Tan(delta) * Math.Sin(epsilon);
            double X = Math.Cos(alpha);
            
            ecl.Lambda = Angle.ToDegrees(Math.Atan2(Y, X));
            ecl.Beta = Angle.ToDegrees(Math.Asin(Math.Sin(delta) * Math.Cos(epsilon) - Math.Cos(delta) * Math.Sin(epsilon) * Math.Sin(alpha)));

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

            double alpha0_alpha = Angle.ToRadians(192.25 - eq.Alpha);
            double delta = Angle.ToRadians(eq.Delta);
            double delta0 = Angle.ToRadians(27.4);
            
            double Y = Math.Sin(alpha0_alpha);
            double X = Math.Cos(alpha0_alpha) * Math.Sin(delta0) - Math.Tan(delta) * Math.Cos(delta0);
            double sinb = Math.Sin(delta) * Math.Sin(delta0) + Math.Cos(delta) * Math.Cos(delta0) * Math.Cos(alpha0_alpha);

            gal.l = Angle.To360(303 - Angle.ToDegrees(Math.Atan2(Y, X)));
            gal.b = Angle.ToDegrees(Math.Asin(sinb));
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

            double l_l0 = Angle.ToRadians(gal.l - 123.0);
            double delta0 = Angle.ToRadians(27.4);
            double b = Angle.ToRadians(gal.b);

            double Y = Math.Sin(l_l0);
            double X = Math.Cos(l_l0) * Math.Sin(delta0) - Math.Tan(b) * Math.Cos(delta0);
            double sinDelta = Math.Sin(b) * Math.Sin(delta0) + Math.Cos(b) * Math.Cos(delta0) * Math.Cos(l_l0);

            eq.Alpha = Angle.To360(Angle.ToDegrees(Math.Atan2(Y, X)) + 12.25);
            eq.Delta = Angle.ToDegrees(Math.Asin(sinDelta));
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

            double beta = Angle.ToRadians(ecl.Beta);
            double lambda = Angle.ToRadians(ecl.Lambda);
            double R = ecl.Distance;

            epsilon = Angle.ToRadians(epsilon);

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

        /// <summary>
        /// Converts heliocentrical coordinates to rectangular topocentrical coordinates. 
        /// </summary>
        /// <param name="planet">Heliocentrical coordinates of a planet</param>
        /// <param name="earth">Heliocentrical coordinates of Earth</param>
        /// <returns>Rectangular topocentrical coordinates of a planet.</returns>
        public static CrdsRectangular ToRectangular(this CrdsHeliocentrical planet, CrdsHeliocentrical earth)
        {
            CrdsRectangular rect = new CrdsRectangular();

            double B = Angle.ToRadians(planet.B);
            double L = Angle.ToRadians(planet.L);
            double R = planet.R;

            double B0 = Angle.ToRadians(earth.B);
            double L0 = Angle.ToRadians(earth.L);
            double R0 = earth.R;

            double cosL = Math.Cos(L);
            double sinL = Math.Sin(L);
            double cosB = Math.Cos(B);
            double sinB = Math.Sin(B);

            double cosL0 = Math.Cos(L0);
            double sinL0 = Math.Sin(L0);
            double cosB0 = Math.Cos(B0);
            double sinB0 = Math.Sin(B0);

            rect.X = R * cosB * cosL - R0 * cosB0 * cosL0;
            rect.Y = R * cosB * sinL - R0 * cosB0 * sinL0;
            rect.Z = R * sinB - R0 * sinB0;

            return rect;
        }

        /// <summary>
        /// Converts rectangular topocentric coordinates of a planet to topocentrical ecliptical coordinates
        /// </summary>
        /// <param name="rect">Rectangular topocentric coordinates of a planet</param>
        /// <returns>Topocentrical ecliptical coordinates of a planet</returns>
        public static CrdsEcliptical ToEcliptical(this CrdsRectangular rect)
        {
            double lambda = Angle.To360(Angle.ToDegrees(Math.Atan2(rect.Y, rect.X)));
            double beta = Angle.ToDegrees(Math.Atan(rect.Z / Math.Sqrt(rect.X * rect.X + rect.Y * rect.Y)));
            double distance = Math.Sqrt(rect.X * rect.X + rect.Y * rect.Y + rect.Z * rect.Z);
            return new CrdsEcliptical(lambda, beta, distance);
        }

        /// <summary>
        /// Calculates topocentric equatorial coordinates of celestial body 
        /// with taking into account correction for parallax.
        /// </summary>
        /// <param name="eq">Geocentric equatorial coordinates of the body</param>
        /// <param name="geo">Geographical coordinates of the body</param>
        /// <param name="theta0">Apparent sidereal time at Greenwich</param>
        /// <param name="pi">Parallax of a body</param>
        /// <returns>Topocentric equatorial coordinates of the celestial body</returns>
        /// <remarks>
        /// Method is taken from AA(II), formulae 40.6-40.7.
        /// </remarks>
        public static CrdsEquatorial ToTopocentric(this CrdsEquatorial eq, CrdsGeographical geo, double theta0, double pi)
        {
            double H = Angle.ToRadians(HourAngle(theta0, geo.Longitude, eq.Alpha));
            double delta = Angle.ToRadians(eq.Delta);
            double sinPi = Math.Sin(Angle.ToRadians(pi));

            double A = Math.Cos(delta) * Math.Sin(H);
            double B = Math.Cos(delta) * Math.Cos(H) - geo.RhoCosPhi * sinPi;
            double C = Math.Sin(delta) - geo.RhoSinPhi * sinPi;

            double q = Math.Sqrt(A * A + B * B + C * C);

            double H_ = Angle.ToDegrees(Math.Atan2(A, B));

            double alpha_ = Angle.To360(theta0 - geo.Longitude - H_);
            double delta_ = Angle.ToDegrees(Math.Asin(C / q));

            return new CrdsEquatorial(alpha_, delta_);
        }

        public static CrdsHorizontal WithRefraction(this CrdsHorizontal h0)
        {
            //if (h0.Altitude < 0) return h0;
            double R = 1.02 / Math.Tan(Angle.ToRadians(h0.Altitude + 10.3 / (h0.Altitude + 5.11)));
            return new CrdsHorizontal(h0.Azimuth, h0.Altitude + R / 60);
        }
    }
}
