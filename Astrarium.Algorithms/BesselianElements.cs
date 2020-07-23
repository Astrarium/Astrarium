using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static Astrarium.Algorithms.Angle;
using static System.Math;

namespace Astrarium.Algorithms
{
    internal class Vector
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Vector() { }

        public Vector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector operator -(Vector v1, Vector v2)
        {
            return new Vector(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static Vector operator +(Vector v1, Vector v2)
        {
            return new Vector(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Vector operator -(Vector v)
        {
            return new Vector(-v.X, -v.Y, -v.Z);
        }

        public static Vector operator *(double n, Vector v)
        {
            return new Vector(n * v.X, n * v.Y, n * v.Z);
        }

        public static Vector operator *(Vector v, double n)
        {
            return new Vector(n * v.X, n * v.Y, n * v.Z);
        }

        public static Vector operator /(Vector v, double n)
        {
            return new Vector(v.X / n, v.Y / n, v.Z / n);
        }

        public static Vector operator *(Matrix m, Vector v)
        {
            return new Vector(
                m.Values[0, 0] * v.X + m.Values[0, 1] * v.Y + m.Values[0, 2] * v.Z,
                m.Values[1, 0] * v.X + m.Values[1, 1] * v.Y + m.Values[1, 2] * v.Z,
                m.Values[2, 0] * v.X + m.Values[2, 1] * v.Y + m.Values[2, 2] * v.Z
            );
        } 

        /// <summary>
        /// Dot product
        /// </summary>
        public static double Dot(Vector v1, Vector v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        /// <summary>
        /// Norm
        /// </summary>
        public static double Norm(Vector v)
        {
            return Sqrt(Dot(v, v));
        }
    }

    /// <summary>
    /// Represents set of Besselian elements describing solar eclipse appearance
    /// </summary>
    public class BesselianElements
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double D { get; set; }
        public double L1 { get; set; }
        public double L2 { get; set; }
        public double Mu { get; set; }

        /// <summary>
        /// Calculates Besselian elements for solar eclipse
        /// </summary>
        /// <param name="jd">Julian day of interest.</param>
        /// <param name="sun">Geocentrical equatorial coordinates of the Sun at the moment</param>
        /// <param name="moon">Geocentrical equatorial coordinates of the Moon at the moment</param>
        /// <param name="rs">Distance Earth-Sun center, in units of Earth equatorial radii.</param>
        /// <param name="rm">Distance Earth-Moon center, in units of Earth equatorial radii.</param>
        /// <returns>
        /// Besselian elements for solar eclipse
        /// </returns>
        /// <remarks>
        /// The method is based on formulae given here:
        /// https://de.wikipedia.org/wiki/Besselsche_Elemente
        /// </remarks>
        public static BesselianElements Calculate(double jd, CrdsEquatorial sun, CrdsEquatorial moon, double rs, double rm)
        {
            // Nutation elements
            var nutation = Nutation.NutationElements(jd);

            // True obliquity
            var epsilon = Date.TrueObliquity(jd, nutation.deltaEpsilon);

            // Greenwich apparent sidereal time 
            double theta = Date.ApparentSiderealTime(jd, nutation.deltaPsi, epsilon);

            double aSun = ToRadians(sun.Alpha);
            double dSun = ToRadians(sun.Delta);

            double aMoon = ToRadians(moon.Alpha);
            double dMoon = ToRadians(moon.Delta);

            // Earth->Sun vector
            var Rs = new Vector(
                rs * Cos(aSun) * Cos(dSun),
                rs * Sin(aSun) * Cos(dSun),
                rs * Sin(dSun)
            );

            // Earth->Moon vector
            var Rm = new Vector(
                rm * Cos(aMoon) * Cos(dMoon),
                rm * Sin(aMoon) * Cos(dMoon),
                rm * Sin(dMoon)
            );

            Vector Rsm = Rs - Rm;

            double lenRsm = Vector.Norm(Rsm);

            // k vector
            Vector k = Rsm / lenRsm;

            double d = Asin(k.Z);
            double a = Atan2(k.Y, k.X);

            double x = rm * Cos(dMoon) * Sin(aMoon - a);
            double y = rm * (Sin(dMoon) * Cos(d) - Cos(dMoon) * Sin(d) * Cos(aMoon - a));
            double zm = rm * (Sin(dMoon) * Sin(d) + Cos(dMoon) * Cos(d) * Cos(aMoon - a));

            // Sun and Moon radii, in Earth equatorial radii
            //
            // Values are taken from "Astronomy on the PC" book, 
            // Oliver Montenbruck, Thomas Pfleger, 
            // Russian edition, p. 189.
            double rhoSun = 218.25 / 2;
            double rhoMoon = 0.5450 / 2;

            double sinF1 = (rhoSun + rhoMoon) / lenRsm;
            double sinF2 = (rhoSun - rhoMoon) / lenRsm;

            double F1 = Asin(sinF1);
            double F2 = Asin(sinF2);

            double zv1 = zm + rhoMoon / sinF1;
            double zv2 = zm - rhoMoon / sinF2;

            double l1 = zv1 * Tan(F1);
            double l2 = zv2 * Tan(F2);

            return new BesselianElements()
            {
                X = x,
                Y = y,
                L1 = l1,
                L2 = l2,
                D = ToDegrees(d),
                Mu = To360(theta - ToDegrees(a))
            };
        }

        public static CrdsGeographical MoonShadowCenter(double jd, CrdsEquatorial sun, CrdsEquatorial moon, double rs, double rm)
        {
            const double fac = 0.996647;     // Ratio polar/equat. Earth radius

            double aSun = ToRadians(sun.Alpha);
            double dSun = ToRadians(sun.Delta);

            double aMoon = ToRadians(moon.Alpha);
            double dMoon = ToRadians(moon.Delta);

            // Earth->Sun vector
            var Rs = new Vector(
                rs * Cos(aSun) * Cos(dSun),
                rs * Sin(aSun) * Cos(dSun),
                rs * Sin(dSun) / fac // Scale z - coordinate to compensate Earth flattening
            );

            // Earth->Moon vector
            var Rm = new Vector(
                rm * Cos(aMoon) * Cos(dMoon),
                rm * Sin(aMoon) * Cos(dMoon),
                rm * Sin(dMoon) / fac // Scale z - coordinate to compensate Earth flattening
            );

            double lenRms = Vector.Norm(Rm - Rs);

            // Shadow axis (Sun->Moon unit vector)
            Vector e = (Rm - Rs) / lenRms;

            // Distance of the Moon from the fundamental plane
            double s0 = -Vector.Dot(Rm, e);

            // Earth radius
            double R_Earth = 6378.137;

            // Distance of the shadow axis from the centre of the Earth
            double Delta = s0 * s0 + R_Earth * R_Earth - Vector.Dot(Rm, Rm);
            double r0 = Sqrt(R_Earth * R_Earth - Delta);

            if (r0 < R_Earth)
            {
                // Intersection of the shadow axis and the surface of the Earth
                double s = s0 - Sqrt(Delta);
                Vector r = Rm + s * e;

                // Re-scale z-component
                r.Z = fac * r.Z;

                // Greenwich sidereal time 
                double theta = Date.MeanSiderealTime(jd);

                // Geographic shadow coordinates

                // Greenwich coordinates
                Vector r_G = Matrix.R3(ToRadians(theta)) * r;


                // East longitude
                double Lambda = To360(ToDegrees(Atan2(r_G.Y, r_G.X) + PI)) - 180;

                double rho = Sqrt(r_G.X * r_G.X + r_G.Y * r_G.Y);

                // Geocentric latitude
                double Phi = Atan2(r_G.Z, rho);

                // Geographic latitude
                Phi = ToDegrees(Phi + 0.1924 * PI / 180 * Sin(2 * Phi));

                return new CrdsGeographical(-Lambda, Phi);
            }
            else
            {
                return null;
            }
        }

        public static CrdsGeographical[] RiseSetCurvePoints(BesselianElements b0)
        {
            return
                CirclesIntersect(new PointF((float)b0.X, (float)b0.Y), b0.L1)
                    .Select(p => ProjectOnEarth(p, b0))
                    .ToArray();
        }

        public static CrdsGeographical[] NorthSouthLimitCurves(BesselianElements b0, BesselianElements b1)
        {
            double dx = b1.X - b0.X;
            double dy = b1.Y - b0.Y;

            double alpha = Atan2(dy, dx);

            double[] angles = new double[] { alpha + PI / 2, alpha - PI / 2 };


            CrdsGeographical[] projections = new CrdsGeographical[2];

            for (int i=0; i<2; i++)
            {
                PointF point = new PointF();
                double angle = angles[i];
                point.X = (float)(b0.X + b0.L1 * Cos(angle));
                point.Y = (float)(b0.Y + b0.L1 * Sin(angle));
                projections[i] = ProjectOnEarth(point, b0);
            }

            return projections;
        }

        /// <summary>
        /// Gets outline points of the Moon shadow cone on the Earth surface
        /// </summary>
        /// <param name="b">Besselian elements</param>
        /// <param name="phase"></param>
        /// <returns></returns>
        public static ICollection<CrdsGeographical> ProjectShadowCone(BesselianElements b)
        {
            List<CrdsGeographical> points = new List<CrdsGeographical>();

            // 360 degrees
            for (int i = 0; i < 360; i++)
            {
                PointF point = new PointF();
                double angle = ToRadians(i);
                point.X = (float)(b.X + b.L2 * Cos(angle));
                point.Y = (float)(b.Y + b.L2 * Sin(angle));

                var projection = ProjectOnEarth(point, b);

                if (projection != null)
                {
                    points.Add(projection);
                }
            }

            return points;
        }

        /// <summary>
        /// Project point from Besselian fundamental plane 
        /// to Earth surface and find geographical coordinates
        /// </summary>
        /// <param name="p">Point on Besselian fundamental plane </param>
        /// <param name="b"></param>
        /// <returns></returns>
        /// <remarks>
        /// Formulae are taken from book
        /// Seidelmann, P. K.: Explanatory Supplement to The Astronomical Almanac, 
        /// University Science Book, Mill Valley (California), 1992,
        /// Chapter 8.3 "Solar Eclipses"
        /// https://archive.org/download/131123ExplanatorySupplementAstronomicalAlmanac/131123-explanatory-supplement-astronomical-almanac.pdf
        /// </remarks>
        public static CrdsGeographical ProjectOnEarth(PointF p, BesselianElements b)
        {
            // Check the point is inside the Earth circle
            if (Abs(p.X * p.X + p.Y * p.Y - 1) < 1e-7)
            {
                // Earth ellipticity, squared
                const double e2 = 0.00669454;

                // 8.334-1
                double rho1 = Sqrt(1 - e2 * Cos(ToRadians(b.D) * Cos(ToRadians(b.D))));

                double xi = p.X;
                double eta = p.Y;

                // 8.333-9
                double eta1 = eta / rho1; 
                               
                // 8.333-10
                double zeta1_2 = 1 - xi * xi - eta1 * eta1;
                double zeta1 = 0;
                if (zeta1_2 > 0)
                {
                    zeta1 = Sqrt(zeta1_2);
                }

                // 8.334-1
                double sind1 = Sin(ToRadians(b.D)) / rho1;
                double cosd1 = Sqrt(1- e2) * Cos(ToRadians(b.D)) / rho1;

                double d1 = Atan2(sind1, cosd1);

                // 8.333-13
                var v = Matrix.R1(d1) * new Vector(xi, eta1, zeta1);
               
                double phi1 = Asin(v.Y);
                double sinTheta = v.X / Cos(phi1);
                double cosTheta = v.Z / Cos(phi1);

                double theta = ToDegrees(Atan2(sinTheta, cosTheta));

                // 8.331-4
                double lambda = b.Mu - theta;

                return new CrdsGeographical(To360(lambda + 180) - 180, ToDegrees(phi1));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Finds points of intersection of two circles.
        /// First circle is a Unit circle (of radius 1 centered at the origin (0, 0) of fundamental plane).
        /// Second circle is defined by its center (<paramref name="p"/>) and radius (<paramref name="r"/>)
        /// </summary>
        /// <param name="p">Center of the second circle</param>
        /// <param name="r">Radius of the second circle</param>
        /// <returns>
        /// Zero, one or two points of intersection
        /// </returns>
        /// <remarks>
        /// Method is based on algorithms
        /// https://e-maxx.ru/algo/circles_intersection
        /// https://e-maxx.ru/algo/circle_line_intersection
        /// </remarks>
        private static PointF[] CirclesIntersect(PointF p, double r)
        {
            double a = -2 * p.X;
            double b = -2 * p.Y;
            double c = p.X * p.X + p.Y * p.Y + 1 - r * r;

            double x0 = -(a * c) / (a * a + b * b);
            double y0 = -(b * c) / (a * a + b * b);

            // no points of intersection
            if (c * c > a * a + b * b + 1e-7)
            {                
                return new PointF[0];
            }
            // one point
            else if (Abs(c * c - (a * a + b * b)) < 1e-7)
            {
                return new PointF[] { new PointF((float)x0, (float)y0) };
            }
            // two points
            else
            {               
                double d = Sqrt(1 - (c * c) / (a * a + b * b));
                double mult = Sqrt((d * d) / (a * a + b * b));
                double ax, ay, bx, by;
                ax = x0 + b * mult;
                ay = y0 - a * mult;
                bx = x0 - b * mult;
                by = y0 + a * mult;

                return new PointF[] { new PointF((float)ax, (float)ay), new PointF((float)bx, (float)by) };
            }
        }                
    }
}
