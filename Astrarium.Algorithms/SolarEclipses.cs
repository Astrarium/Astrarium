using System;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using static System.Math;
using static Astrarium.Algorithms.Angle;

[assembly: InternalsVisibleTo("Astrarium.Algorithms.Tests")]

namespace Astrarium.Algorithms
{
    /// <summary>
    /// Contains methods for calculating solar eclpses
    /// </summary>
    public static class SolarEclipses
    {
        /// <summary>
        /// Finds polynomial Besselian elements by 5 positions of Sun and Moon
        /// </summary>
        /// <param name="positions">Positions of Sun and Moon</param>
        /// <returns>Polynomial Besselian elements of the Solar eclipse</returns>
        public static PolynomialBesselianElements FindPolynomialBesselianElements(SunMoonPosition[] positions)
        {
            if (positions.Length != 5)
                throw new ArgumentException("Five positions are required", nameof(positions));

            double step = positions[1].JulianDay - positions[0].JulianDay;

            if (!positions.Zip(positions.Skip(1), 
                (a, b) => new { a, b })
                .All(p => Abs(p.b.JulianDay - p.a.JulianDay - step) <= 1e-6))
            {
                throw new ArgumentException("Positions should be sorted ascending by JulianDay value, and have same JulianDay step.", nameof(positions));
            }                

            // 5 time instants required
            InstantBesselianElements[] elements = new InstantBesselianElements[5];

            PointF[] points = new PointF[5];
            for (int i = 0; i < 5; i++)
            {
                elements[i] = FindInstantBesselianElements(positions[i]);
                points[i].X = i - 2;
            }

            // Mu expressed in degrees and can cross zero point.
            // Values must be aligned in order to avoid crossing.
            double[] Mu = elements.Select(e => e.Mu).ToArray();
            Angle.Align(Mu);
            for (int i = 0; i < 5; i++)
            {
                elements[i].Mu = Mu[i];
            }

            return new PolynomialBesselianElements()
            {
                JulianDay0 = positions[2].JulianDay,
                Step = step,
                X = LeastSquares.FindCoeffs(points.Select((p, i) => new PointF(p.X, (float)elements[i].X)), 3),
                Y = LeastSquares.FindCoeffs(points.Select((p, i) => new PointF(p.X, (float)elements[i].Y)), 3),
                L1 = LeastSquares.FindCoeffs(points.Select((p, i) => new PointF(p.X, (float)elements[i].L1)), 3),
                L2 = LeastSquares.FindCoeffs(points.Select((p, i) => new PointF(p.X, (float)elements[i].L2)), 3),
                D = LeastSquares.FindCoeffs(points.Select((p, i) => new PointF(p.X, (float)elements[i].D)), 3),
                Mu = LeastSquares.FindCoeffs(points.Select((p, i) => new PointF(p.X, (float)elements[i].Mu)), 3)
            };
        }

        /// <summary>
        /// Calculates Besselian elements for solar eclipse,
        /// valid only for specified instant.
        /// </summary>
        /// <param name="position">Sun and Moon position data</param>
        /// <returns>
        /// Besselian elements for solar eclipse
        /// </returns>
        /// <remarks>
        /// The method is based on formulae given here:
        /// https://de.wikipedia.org/wiki/Besselsche_Elemente
        /// </remarks>
        internal static InstantBesselianElements FindInstantBesselianElements(SunMoonPosition position)
        {
            // Nutation elements
            var nutation = Nutation.NutationElements(position.JulianDay);

            // True obliquity
            var epsilon = Date.TrueObliquity(position.JulianDay, nutation.deltaEpsilon);

            // Greenwich apparent sidereal time 
            double theta = Date.ApparentSiderealTime(position.JulianDay, nutation.deltaPsi, epsilon);

            double aSun = ToRadians(position.Sun.Alpha);
            double dSun = ToRadians(position.Sun.Delta);

            double aMoon = ToRadians(position.Moon.Alpha);
            double dMoon = ToRadians(position.Moon.Delta);

            // Earth->Sun vector
            var Rs = new Vector(
                position.DistanceSun * Cos(aSun) * Cos(dSun),
                position.DistanceSun * Sin(aSun) * Cos(dSun),
                position.DistanceSun * Sin(dSun)
            );

            // Earth->Moon vector
            var Rm = new Vector(
                position.DistanceMoon * Cos(aMoon) * Cos(dMoon),
                position.DistanceMoon * Sin(aMoon) * Cos(dMoon),
                position.DistanceMoon * Sin(dMoon)
            );

            Vector Rsm = Rs - Rm;

            double lenRsm = Vector.Norm(Rsm);

            // k vector
            Vector k = Rsm / lenRsm;

            double d = Asin(k.Z);
            double a = Atan2(k.Y, k.X);

            double x = position.DistanceMoon * Cos(dMoon) * Sin(aMoon - a);
            double y = position.DistanceMoon * (Sin(dMoon) * Cos(d) - Cos(dMoon) * Sin(d) * Cos(aMoon - a));
            double zm = position.DistanceMoon * (Sin(dMoon) * Sin(d) + Cos(dMoon) * Cos(d) * Cos(aMoon - a));

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

            return new InstantBesselianElements()
            {
                X = x,
                Y = y,
                L1 = l1,
                L2 = l2,
                D = ToDegrees(d),
                Mu = To360(theta - ToDegrees(a))
            };
        }

        /// <summary>
        /// Gets map curves of solar eclipse
        /// </summary>
        /// <param name="pbe">Polynomial Besselian elements defining the Eclipse</param>
        /// <returns><see cref="SolarEclipseCurves"/> instance.</returns>
        public static SolarEclipseCurves GetCurves(PolynomialBesselianElements pbe)
        {
            double jdFrom = pbe.From;
            double jdTo = pbe.To;
            double step = TimeSpan.FromMinutes(1).TotalDays;

            SolarEclipseCurves curves = new SolarEclipseCurves();

            // inclination of shadow path
            // respect to fundamental plane
            double inc = 0;

            for (double jd = jdFrom; jd <= jdTo; jd += step)
            {
                InstantBesselianElements b = pbe.GetInstantBesselianElements(jd);

                // Projection of Moon shadow center on fundamental plane
                PointF pCenter = new PointF((float)b.X, (float)b.Y);

                // Umbra center coordinates on Earth surface
                CrdsGeographical umbraCenter = ProjectOnEarth(pCenter, b.D, b.Mu);
                              
                if (umbraCenter != null)
                {
                    curves.UmbraPath.Add(umbraCenter);
                }
                
                // Find penumbra (L1 radius) intersection with
                // Earth circle on fundamental plane
                PointF[] pPenumbraIntersect = CirclesIntersection(pCenter, b.L1);
                
                // Find corresponding geographical coordinates
                CrdsGeographical[] gPenumbraIntersect = pPenumbraIntersect
                    .Select(p => ProjectOnEarth(p, b.D, b.Mu))
                    .Where(p => p != null).ToArray();

                if (gPenumbraIntersect.Any())
                {
                    // Rise curve
                    if (jd <= jdFrom + (jdTo - jdFrom) / 2)
                    {
                        curves.RiseCurve.AddMany(gPenumbraIntersect);                        
                    }

                    // Set curve
                    if (jd >= jdFrom + (jdTo - jdFrom) / 2)
                    {
                        curves.SetCurve.AddMany(gPenumbraIntersect);
                    }
                }
                             
                // look up to next time
                if (jdTo - jd > step)
                {
                    InstantBesselianElements bNext = pbe.GetInstantBesselianElements(jd + step);
                    double dx = bNext.X - b.X;
                    double dy = bNext.Y - b.Y;
                    inc = Atan2(dy, dx);
                }

                // Find northern and southern umbra and penumbra limit points
                double[] angles = new double[] { inc + PI / 2, inc - PI / 2 };
                for (int i = 0; i < 2; i++)
                {
                    double angle = angles[i];
                    
                    var pPenumbra = new PointF(
                        (float)(b.X + b.L1 * Cos(angle)),
                        (float)(b.Y + b.L1 * Sin(angle)));

                    var pUmbra = new PointF(
                        (float)(b.X + b.L2 * Cos(angle)),
                        (float)(b.Y + b.L2 * Sin(angle)));

                    var penumbraLimit = ProjectOnEarth(pPenumbra, b.D, b.Mu);
                    var umbraLimit = ProjectOnEarth(pUmbra, b.D, b.Mu);

                    if (penumbraLimit != null)
                    {
                        if (i == 0)
                            curves.PenumbraNorthernLimit.Add(penumbraLimit);
                        else
                            curves.PenumbraSouthernLimit.Add(penumbraLimit);
                    }

                    if (umbraLimit != null)
                    {
                        if (i == 0)
                            curves.UmbraNorthernLimit.Add(umbraLimit);
                        else
                            curves.UmbraSouthernLimit.Add(umbraLimit);
                    }
                }
            }

            return curves;
        }
      
        /// <summary>
        /// Project point from Besselian fundamental plane 
        /// to Earth surface and find geographical coordinates of projection
        /// </summary>
        /// <param name="p">Point on Besselian fundamental plane</param>
        /// <param name="d">Declination of Moon shadow vector, in degrees</param>
        /// <param name="mu">Hour angle of Moon shadow vector, in degrees</param>
        /// <returns>
        /// Geograhphical coordinates of a point on Earth surface, corresponding to the
        /// point on Besselian fundamental plane, or null if point is outside the Earth circle on the plane.
        /// </returns>
        /// <remarks>
        /// Formulae are taken from book
        /// Seidelmann, P. K.: Explanatory Supplement to The Astronomical Almanac, 
        /// University Science Book, Mill Valley (California), 1992,
        /// Chapter 8.3 "Solar Eclipses"
        /// https://archive.org/download/131123ExplanatorySupplementAstronomicalAlmanac/131123-explanatory-supplement-astronomical-almanac.pdf
        /// </remarks>
        private static CrdsGeographical ProjectOnEarth(PointF p, double d, double mu)
        {
            // Check the point is inside the Earth circle
            if (p.X * p.X + p.Y * p.Y <= 1)
            {
                // Earth ellipticity, squared
                const double e2 = 0.00669454;

                // 8.334-1
                double rho1 = Sqrt(1 - e2 * Cos(ToRadians(d) * Cos(ToRadians(d))));

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
                double sind1 = Sin(ToRadians(d)) / rho1;
                double cosd1 = Sqrt(1 - e2) * Cos(ToRadians(d)) / rho1;

                double d1 = Atan2(sind1, cosd1);

                // 8.333-13
                var v = Matrix.R1(d1) * new Vector(xi, eta1, zeta1);

                double phi1 = Asin(v.Y);
                double sinTheta = v.X / Cos(phi1);
                double cosTheta = v.Z / Cos(phi1);

                double theta = ToDegrees(Atan2(sinTheta, cosTheta));

                // 8.331-4
                double lambda = mu - theta;

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
        private static PointF[] CirclesIntersection(PointF p, double r)
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
