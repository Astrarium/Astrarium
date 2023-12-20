using System;

namespace Astrarium.Algorithms
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
        /// <param name="theta0">Sidereal time at Greenwich</param>
        /// <remarks>
        /// Implementation is taken from AA(I), formulae 12.5, 12.6.
        /// </remarks>
        public static CrdsHorizontal ToHorizontal(this CrdsEquatorial eq, CrdsGeographical geo, double theta0)
        {
            double H = Angle.ToRadians(HourAngle(theta0, geo.Longitude, eq.Alpha));
            double phi = Angle.ToRadians(geo.Latitude);
            double delta = Angle.ToRadians(eq.Delta);

            double sinH = Math.Sin(H);
            double cosH = Math.Cos(H);
            double sinPhi = Math.Sin(phi);
            double cosPhi = Math.Cos(phi);
            double sinDelta = Math.Sin(delta);
            double cosDelta = Math.Cos(delta);
            double tanDelta = Math.Tan(delta);

            double Y = sinH;
            double X = cosH * sinPhi - tanDelta * cosPhi;

            CrdsHorizontal hor = new CrdsHorizontal();

            hor.Altitude = Angle.ToDegrees(Math.Asin(sinPhi * sinDelta + cosPhi * cosDelta * cosH));

            hor.Azimuth = Angle.ToDegrees(Math.Atan2(Y, X));
            hor.Azimuth = Angle.To360(hor.Azimuth);

            return hor;
        }

        /// <summary>
        /// Converts local horizontal coordinates to equatorial coordinates. 
        /// </summary>
        /// <param name="hor">Pair of local horizontal coordinates.</param>
        /// <param name="geo">Geographical of the observer</param>
        /// <param name="theta0">Sidereal time at Greenwich.</param>
        /// <returns>Pair of equatorial coordinates</returns>
        public static CrdsEquatorial ToEquatorial(this CrdsHorizontal hor, CrdsGeographical geo, double theta0)
        {
            double A = Angle.ToRadians(hor.Azimuth);
            double h = Angle.ToRadians(hor.Altitude);
            double phi = Angle.ToRadians(geo.Latitude);

            double sinPhi = Math.Sin(phi);
            double cosPhi = Math.Cos(phi);
            double sinA = Math.Sin(A);
            double cosA = Math.Cos(A);
            double sinH = Math.Sin(h);
            double cosH = Math.Cos(h);

            double Y = sinA;
            double X = cosA * sinPhi + Math.Tan(h) * cosPhi;

            double H = Angle.ToDegrees(Math.Atan2(Y, X));

            CrdsEquatorial eq = new CrdsEquatorial();
            eq.Alpha = Angle.To360(theta0 - geo.Longitude - H);
            eq.Delta = Angle.ToDegrees(Math.Asin(sinPhi * sinH - cosPhi * cosH * cosA));

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

            // TODO: optimize

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
            // TODO: optimize

            CrdsEcliptical ecl = new CrdsEcliptical();

            epsilon = Angle.ToRadians(epsilon);
            double alpha = Angle.ToRadians(eq.Alpha);
            double delta = Angle.ToRadians(eq.Delta);

            double Y = Math.Sin(alpha) * Math.Cos(epsilon) + Math.Tan(delta) * Math.Sin(epsilon);
            double X = Math.Cos(alpha);
            
            ecl.Lambda = Angle.To360(Angle.ToDegrees(Math.Atan2(Y, X)));
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

            // TODO: optimize

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

            double sinDelta0 = Math.Sin(delta0);
            double cosDelta0 = Math.Cos(delta0);
            double cosL_l0 = Math.Cos(l_l0);

            double Y = Math.Sin(l_l0);
            double X = cosL_l0 * sinDelta0 - Math.Tan(b) * cosDelta0;
            double sinDelta = Math.Sin(b) * sinDelta0 + Math.Cos(b) * cosDelta0 * cosL_l0;

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
            // TODO: optimize

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
            // TODO: optimize

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

        /// <summary>
        /// Converts rectangular planetocentrical coordinates of satellite to equatorial coordinates as seen from Earth
        /// </summary>
        /// <param name="m">Rectangular planetocentrical coordinates of satellite</param>
        /// <param name="planet">Equatorial coordinates of parent planet (as seen from Earth)</param>
        /// <param name="P">Position angle of parent planet</param>
        /// <param name="semidiameter">Visible semidiameter of parent planet, in seconds of arc.</param>
        /// <returns>Equatorial coordinates of satellite as seen from Earth</returns>
        /// <remarks>
        /// The method is taken from geometrical drawing (own work)
        /// </remarks>
        public static CrdsEquatorial ToEquatorial(this CrdsRectangular m, CrdsEquatorial planet, double P, double semidiameter)
        {
            // TODO: optimize

            // convert rectangular planetocentrical coordinates to planetocentrical polar coordinates

            // radius-vector of moon, in planet's equatorial radii
            double r = Math.Sqrt(m.X * m.X + m.Y * m.Y);

            // rotation angle
            double theta = Angle.ToDegrees(Math.Atan2(m.Y, m.X));

            // rotate with position angle of the planet
            theta += P;

            // convert back to rectangular coordinates, but rotated with P angle:
            double x = r * Math.Cos(Angle.ToRadians(theta));
            double y = r * Math.Sin(Angle.ToRadians(theta));

            // delta of RA
            double dAlpha = (1 / Math.Cos(Angle.ToRadians(planet.Delta))) * x * semidiameter / 3600;
            
            // delta of Declination
            // negative sign because positive delta means southward direction
            double dDelta = -y * semidiameter / 3600;

            return new CrdsEquatorial(planet.Alpha - dAlpha, planet.Delta - dDelta);
        }

        /// <summary>
        /// Calculates distance in kilometers between 2 points on the Earth surface.
        /// </summary>
        /// <param name="g1">First point.</param>
        /// <param name="g2">Second point.</param>
        /// <returns>Distance in kilometers between 2 points.</returns>
        /// <remarks>
        /// The method is taken from https://www.movable-type.co.uk/scripts/latlong.html
        /// </remarks>
        public static double DistanceTo(this CrdsGeographical g1, CrdsGeographical g2)
        {
            const double R = 6371;
            double phi1 = Angle.ToRadians(g1.Latitude);
            double phi2 = Angle.ToRadians(g2.Latitude);
            double deltaPhi = Angle.ToRadians(g2.Latitude - g1.Latitude);
            double deltaLambda = Angle.ToRadians(g2.Longitude - g1.Longitude);
            double a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) + Math.Cos(phi1) * Math.Cos(phi2) * Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}
