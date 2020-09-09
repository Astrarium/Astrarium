using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            // Calculate Inc
            for (int i = 0; i < 4; i++)
            {
                elements[i].Inc = ToDegrees(Atan2(elements[i + 1].Y - elements[i].Y, elements[i + 1].X - elements[i].X));
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
                Mu = LeastSquares.FindCoeffs(points.Select((p, i) => new PointF(p.X, (float)elements[i].Mu)), 3),
                Inc = LeastSquares.FindCoeffs(points.Take(4).Select((p, i) => new PointF(p.X, (float)elements[i].Inc)), 3),
                F1 = LeastSquares.FindCoeffs(points.Select((p, i) => new PointF(p.X, (float)elements[i].F1)), 3),
                F2 = LeastSquares.FindCoeffs(points.Select((p, i) => new PointF(p.X, (float)elements[i].F2)), 3)
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
                Mu = To360(theta - ToDegrees(a)),
                F1 = ToDegrees(F1),
                F2 = ToDegrees(F2)
            };
        }

        /// <summary>
        /// Gets map of solar eclipse.
        /// </summary>
        /// <param name="pbe">Polynomial Besselian elements defining the Eclipse</param>
        /// <returns><see cref="SolarEclipseMap"/> instance.</returns>
        public static SolarEclipseMap GetEclipseMap(PolynomialBesselianElements pbe)
        {
            // left edge of time interval
            double jdFrom = pbe.From;

            // midpoint of time interval
            double jdMid = pbe.From + (pbe.To - pbe.From) / 2;

            // right edge of time interval
            double jdTo = pbe.To;

            // precision of calculation, in days
            double epsilon = 1e-8;

            // Eclipse map data
            SolarEclipseMap map = new SolarEclipseMap();

            // Function has zero value when umbra center crosses Earth edge
            Func<double, double> funcUmbra = (jd) =>
            {
                var b = pbe.GetInstantBesselianElements(jd);
                return Sqrt(b.X * b.X + b.Y * b.Y) - 1 - Abs(b.L2);
            };

            // Function has zero value when penumbra edge crosses Earth edge externally
            Func<double, double> funcExternalContact = (jd) =>
            {
                var b = pbe.GetInstantBesselianElements(jd);
                return Sqrt(b.X * b.X + b.Y * b.Y) - 1 - b.L1;
            };

            // Function has zero value when penumbra edge crosses Earth edge internally
            Func<double, double> funcInternalContact = (jd) =>
            {
                var b = pbe.GetInstantBesselianElements(jd);
                return Sqrt(b.X * b.X + b.Y * b.Y) - 1 + b.L1;
            };

            // Function has zero value when northern limit of penumbra crosses Earth edge
            Func<double, double> funcPenumbraNorthLimit = (jd) =>
            {
                var b = pbe.GetInstantBesselianElements(jd);
                double angle = ToRadians(b.Inc + 90);
                var p = new PointF(
                        (float)(b.X + b.L1 * Cos(angle)),
                        (float)(b.Y + b.L1 * Sin(angle)));

                return Sqrt(p.X * p.X + p.Y * p.Y) - 1;
            };

            // Function has zero value when southern limit of penumbra crosses Earth edge
            Func<double, double> funcPenumbraSouthLimit = (jd) =>
            {
                var b = pbe.GetInstantBesselianElements(jd);
                double angle = ToRadians(b.Inc - 90);
                var p = new PointF(
                        (float)(b.X + b.L1 * Cos(angle)),
                        (float)(b.Y + b.L1 * Sin(angle)));

                return Sqrt(p.X * p.X + p.Y * p.Y) - 1;
            };

            // Function has zero value when northern limit of umbra crosses Earth edge
            Func<double, double> funcUmbraNorthLimit = (jd) =>
            {
                var b = pbe.GetInstantBesselianElements(jd);
                double angle = ToRadians(b.Inc + 90);
                var p = new PointF(
                        (float)(b.X + Abs(b.L2) * Cos(angle)),
                        (float)(b.Y + Abs(b.L2) * Sin(angle)));

                return Sqrt(p.X * p.X + p.Y * p.Y) - 1;
            };

            // Function has zero value when southern limit of umbra crosses Earth edge
            Func<double, double> funcUmbraSouthLimit = (jd) =>
            {
                var b = pbe.GetInstantBesselianElements(jd);
                double angle = ToRadians(b.Inc - 90);
                var p = new PointF(
                        (float)(b.X + Abs(b.L2) * Cos(angle)),
                        (float)(b.Y + Abs(b.L2) * Sin(angle)));

                return Sqrt(p.X * p.X + p.Y * p.Y) - 1;
            };

            // Instant of first external contact of penumbra,
            // assume always exists
            double jdP1 = FindRoots(funcExternalContact, jdFrom, jdMid, epsilon);
            {
                InstantBesselianElements b = pbe.GetInstantBesselianElements(jdP1);
                double a = Atan2(b.Y, b.X);
                PointF p = new PointF((float)Cos(a), (float)Sin(a));                
                map.P1 = new SolarEclipsePoint(jdP1, ProjectOnEarth(p, b.D, b.Mu, true));
            }

            // Instant of last external contact of penumbra
            // assume always exists
            double jdP4 = FindRoots(funcExternalContact, jdMid, jdTo, epsilon);
            {
                InstantBesselianElements b = pbe.GetInstantBesselianElements(jdP4);
                double a = Atan2(b.Y, b.X);
                PointF p = new PointF((float)Cos(a), (float)Sin(a));
                map.P4 = new SolarEclipsePoint(jdP4, ProjectOnEarth(p, b.D, b.Mu, true));
            }

            // Instant of first internal contact of penumbra,
            // may not exist
            double jdP2 = FindRoots(funcInternalContact, jdFrom, jdMid, epsilon);
            if (!double.IsNaN(jdP2))
            {
                InstantBesselianElements b = pbe.GetInstantBesselianElements(jdP2);
                double a = Atan2(b.Y, b.X);
                PointF p = new PointF((float)Cos(a), (float)Sin(a));
                map.P2 = new SolarEclipsePoint(jdP2, ProjectOnEarth(p, b.D, b.Mu, true));
            }

            // Instant of last internal contact of penumbra,
            // may not exist
            double jdP3 = FindRoots(funcInternalContact, jdMid, jdTo, epsilon);
            if (!double.IsNaN(jdP3))
            {
                InstantBesselianElements b = pbe.GetInstantBesselianElements(jdP3);
                double a = Atan2(b.Y, b.X);
                PointF p = new PointF((float)Cos(a), (float)Sin(a));
                map.P3 = new SolarEclipsePoint(jdP3, ProjectOnEarth(p, b.D, b.Mu, true));
            }

            // Instant when northern limit of penumbra crosses Earth edge first time,
            // may not exist
            double jdPN1 = FindRoots(funcPenumbraNorthLimit, jdFrom, jdMid, epsilon);
            if (!double.IsNaN(jdPN1))
            {
                //InstantBesselianElements b = pbe.GetInstantBesselianElements(jdPN1);
                //PointF p = CirclesIntersection(new PointF((float)b.X, (float)b.Y), b.L1)[0];
                //map.PN1 = new SolarEclipsePoint(jdPN1, ProjectOnEarth(p, b.D, b.Mu));
            }

            // Instant when northern limit of penumbra crosses Earth edge last time,
            // may not exist
            double jdPN2 = FindRoots(funcPenumbraNorthLimit, jdMid, jdTo, epsilon);
            if (!double.IsNaN(jdPN2))
            {
                //InstantBesselianElements b = pbe.GetInstantBesselianElements(jdPN2);
                //PointF p = CirclesIntersection(new PointF((float)b.X, (float)b.Y), b.L1)[0];
                //map.PN2 = new SolarEclipsePoint(jdPN2, ProjectOnEarth(p, b.D, b.Mu));
            }

            // Instant when southern limit of penumbra crosses Earth edge first time,
            // may not exist
            double jdPS1 = FindRoots(funcPenumbraSouthLimit, jdFrom, jdMid, epsilon);
            if (!double.IsNaN(jdPS1))
            {
                //InstantBesselianElements b = pbe.GetInstantBesselianElements(jdPS1);
                //PointF p = CirclesIntersection(new PointF((float)b.X, (float)b.Y), b.L1)[1];
                //map.PS1 = new SolarEclipsePoint(jdPS1, ProjectOnEarth(p, b.D, b.Mu));
            }

            // Instant when southern limit of penumbra crosses Earth edge last time,
            // may not exist
            double jdPS2 = FindRoots(funcPenumbraSouthLimit, jdMid, jdTo, epsilon);
            if (!double.IsNaN(jdPS2))
            {
                //InstantBesselianElements b = pbe.GetInstantBesselianElements(jdPS2);
                //PointF p = CirclesIntersection(new PointF((float)b.X, (float)b.Y), b.L1)[1];
                //map.PS2 = new SolarEclipsePoint(jdPS2, ProjectOnEarth(p, b.D, b.Mu));
            }

            // Instant when northern limit of umbra crosses Earth edge first time,
            // may not exist
            double jdUN1 = FindRoots(funcUmbraNorthLimit, jdFrom, jdMid, epsilon);
            
            // Instant when northern limit of umbra crosses Earth edge last time,
            // may not exist
            double jdUN2 = FindRoots(funcUmbraNorthLimit, jdMid, jdTo, epsilon);
            
            // Instant when southern limit of umbra crosses Earth edge first time,
            // may not exist
            double jdUS1 = FindRoots(funcUmbraSouthLimit, jdFrom, jdMid, epsilon);
           
            // Instant when southern limit of umbra crosses Earth edge last time,
            // may not exist
            double jdUS2 = FindRoots(funcUmbraSouthLimit, jdMid, jdTo, epsilon);

            // Instant of first contact of umbra center,
            // may not exist
            double jdC1 = FindRoots(funcUmbra, jdFrom, jdMid, epsilon);
            if (!double.IsNaN(jdC1))
            {
                do
                {
                    InstantBesselianElements b = pbe.GetInstantBesselianElements(jdC1);
                    PointF p = new PointF((float)b.X, (float)b.Y);
                    var g = ProjectOnEarth(p, b.D, b.Mu);
                    if (g == null) 
                        jdC1 += 1e-6;
                    else
                        map.C1 = new SolarEclipsePoint(jdC1, g);
                }
                while (map.C1 == null && jdC1 < jdMid);
            }

            // Instant of last contact of umbra center,
            // may not exist
            double jdC2 = FindRoots(funcUmbra, jdMid, jdTo, epsilon);
            if (!double.IsNaN(jdC2))
            {
                do
                {
                    InstantBesselianElements b = pbe.GetInstantBesselianElements(jdC2);
                    PointF p = new PointF((float)b.X, (float)b.Y);
                    var g = ProjectOnEarth(p, b.D, b.Mu);
                    if (g == null)
                        jdC2 -= 1e-6;
                    else
                        map.C2 = new SolarEclipsePoint(jdC2, g);
                }
                while (map.C2 == null && jdC2 > jdMid);
            }

            // Find points of northern limit of eclipse visibility
            FindPenumbraLimits(pbe, map.PenumbraNorthernLimit, jdPN1, jdPN2, 90);

            // Find points of southern limit of eclipse visibility
            FindPenumbraLimits(pbe, map.PenumbraSouthernLimit, jdPS1, jdPS2, -90);

            // Find points of northern limit of total eclipse visibility
            FindUmbraLimits(pbe, map.UmbraNorthernLimit, jdUN1, jdUN2, 90);

            // Find points of southern limit of total eclipse visibility
            FindUmbraLimits(pbe, map.UmbraSouthernLimit, jdUS1, jdUS2, -90);

            // Calc rise/set curves
            FindRiseSetCurves(pbe, map, jdP1, jdP4);

            // Calc umbra track points
            FindTotalPath(pbe, map, jdC1, jdC2);
            
            return map;
        }

        private static void FindPenumbraLimits(PolynomialBesselianElements pbe, ICollection<CrdsGeographical> curve, double jdFrom, double jdTo, double ang)
        {
            if (!double.IsNaN(jdFrom) && !double.IsNaN(jdTo))
            {
                double step0 = FindStep(jdTo - jdFrom);
                double step = step0;

                for (double jd = jdFrom; jd <= jdTo + step * 0.1; jd += step)
                {
                    InstantBesselianElements b = pbe.GetInstantBesselianElements(jd);

                    double angle = ToRadians(b.Inc + ang);
                  
                    PointF p = new PointF(
                        (float)(b.X + b.L1 * Cos(angle)),
                        (float)(b.Y + b.L1 * Sin(angle)));

                    double z2 = 1 - p.X * p.X - p.Y * p.Y;
                    if (z2 > 1) z2 = 1;
                    if (z2 < 0) z2 = 0;
                    double z = Sqrt(z2);
                    double r = b.L1 - z * Tan(ToRadians(b.F1));
                    
                    p = new PointF(
                        (float)(b.X + r * Cos(angle)),
                        (float)(b.Y + r * Sin(angle)));

                    var g = ProjectOnEarth(p, b.D, b.Mu, true);

                    if (g != null)
                    {
                        curve.Add(g);
                    }
                }
            }
        }

        private static void FindUmbraLimits(PolynomialBesselianElements pbe, ICollection<CrdsGeographical>[] curve, double jdFrom, double jdTo, double ang)
        {
            if (!double.IsNaN(jdFrom) && !double.IsNaN(jdTo))
            {
                int c = 0;
                double step0 = FindStep(jdTo - jdFrom);
                double step = step0;

                for (double jd = jdFrom; jd <= jdTo + step * 0.5; jd += step)
                {
                    InstantBesselianElements b = pbe.GetInstantBesselianElements(jd);

                    double angle = ToRadians(b.Inc + ang);

                    var p = new PointF(
                        (float)(b.X + Abs(b.L2) * Cos(angle)),
                        (float)(b.Y + Abs(b.L2) * Sin(angle)));

                    double z2 = 1 - p.X * p.X - p.Y * p.Y;
                    if (z2 > 1) z2 = 1;
                    if (z2 < 0) z2 = 0;
                    double z = Sqrt(z2);
                    double r = Abs(b.L2 - z * Tan(ToRadians(b.F2)));

                    p = new PointF(
                        (float)(b.X + r * Cos(angle)),
                        (float)(b.Y + r * Sin(angle)));

                    // Adjust iteration step according to penumbra position
                    if (Abs(p.Y) > 0.9)
                    {
                        step = step0 / 4;
                    }
                    else
                    {
                        step = step0;
                    }

                    CrdsGeographical g = ProjectOnEarth(p, b.D, b.Mu);

                    if (g != null)
                    {
                        if (c == 0 && Abs(g.Latitude) > 85.5)
                        {
                            curve[0].Add(g);
                            c = 1;
                        }
                        curve[c].Add(g);
                    }
                }
            }
        }

        private static void FindRiseSetCurves(PolynomialBesselianElements pbe, SolarEclipseMap map, double jdFrom, double jdTo)
        {
            double step0 = FindStep(jdTo - jdFrom);
            double step = step0;
            int riseSet = 0;

            for (double jd = jdFrom; jd <= jdTo + step * 0.1; jd += step)
            {
                InstantBesselianElements b = pbe.GetInstantBesselianElements(jd);

                // Projection of Moon shadow center on fundamental plane
                PointF pCenter = new PointF((float)b.X, (float)b.Y);

                // Find penumbra (L1 radius) intersection with
                // Earth circle on fundamental plane
                PointF[] pPenumbraIntersect = CirclesIntersection(pCenter, b.L1);

                // Adjust iteration step according to current penumbra projection
                if (Abs(b.X) < 0.15 && pPenumbraIntersect.Any(p => Abs(p.Y) > 0.95))
                {
                    step = step0 / 10;
                }
                else
                {
                    step = step0;
                }

                if (Abs(jd - map.P1.JulianDay) < step)
                {
                    CrdsGeographical g = map.P1.Coordinates;
                    if (pCenter.X <= 0)
                        map.RiseSetCurve[riseSet].Insert(0, g);
                    else
                        map.RiseSetCurve[riseSet].Add(g);
                }
                else if (map.P2 != null && Abs(jd - map.P2.JulianDay) < step)
                {
                    CrdsGeographical g = map.P2.Coordinates;
                    if (pCenter.X <= 0)
                        map.RiseSetCurve[riseSet].Insert(0, g);
                    else
                        map.RiseSetCurve[riseSet].Add(g);
                }
                else if (map.P3 != null && Abs(jd - map.P3.JulianDay) < step)
                {
                    CrdsGeographical g = map.P3.Coordinates;
                    if (pCenter.X <= 0)
                        map.RiseSetCurve[riseSet].Insert(0, g);
                    else
                        map.RiseSetCurve[riseSet].Add(g);
                }
                else if (Abs(jd - map.P4.JulianDay) < step)
                {
                    CrdsGeographical g = map.P4.Coordinates;
                    if (pCenter.X <= 0)
                        map.RiseSetCurve[riseSet].Insert(0, g);
                    else
                        map.RiseSetCurve[riseSet].Add(g);
                }
                else
                {
                    for (int i = 0; i < pPenumbraIntersect.Length; i++)
                    {
                        CrdsGeographical g = ProjectOnEarth(pPenumbraIntersect[i], b.D, b.Mu, forceProjection: true);

                        //if (g == null) continue;

                        if (pCenter.X <= 0)
                        {
                            if (i == 0)
                                map.RiseSetCurve[riseSet].Insert(0, g);
                            else
                                map.RiseSetCurve[riseSet].Add(g);
                        }
                        else
                        {
                            if (i == 0)
                                map.RiseSetCurve[riseSet].Add(g);
                            else
                                map.RiseSetCurve[riseSet].Insert(0, g);
                        }
                    }
                }

                // Penumbra is totally inside Earth circle
                if (map.PenumbraNorthernLimit.Count > 0 &&
                    map.PenumbraSouthernLimit.Count > 0 &&
                    pCenter.X * pCenter.X + pCenter.Y * pCenter.Y < 1 &&
                    !pPenumbraIntersect.Any())
                {
                    riseSet = 1;
                }
            }
        }

        private static void FindTotalPath(PolynomialBesselianElements pbe, SolarEclipseMap curves, double jdFrom, double jdTo)
        {
            if (!double.IsNaN(jdFrom) && !double.IsNaN(jdTo))
            {
                int c = 0;

                double step0 = FindStep(jdTo - jdFrom);
                double step = step0;

                for (double jd = jdFrom; jd <= jdTo + step * 0.1; jd += step)
                {
                    InstantBesselianElements b = pbe.GetInstantBesselianElements(jd);

                    // Adjust iteration step according to current umbra projection
                    if (Abs(b.Y) > 0.8)
                    {
                        step = step0 / 4;
                    }
                    else
                    {
                        step = step0;
                    }

                    // Projection of Moon shadow center on fundamental plane
                    PointF pCenter = new PointF((float)b.X, (float)b.Y);

                    // Umbra center coordinates on Earth surface
                    CrdsGeographical g = ProjectOnEarth(pCenter, b.D, b.Mu);

                    if (g != null)
                    {
                        if (c == 0 && Abs(g.Latitude) > 89.9)
                        {
                            curves.TotalPath[0].Add(g);
                            c = 1;
                        }
                        curves.TotalPath[c].Add(g);
                    }
                }
            } 
        }

        /// <summary>
        /// Finds step value (in Julian days) needed for calculating curve points.
        /// </summary>
        /// <param name="deltaJd">Time interval, in Julian days</param>
        /// <returns>Step value (in Julian days, closest to 1 minute) needed for calculating curve points.</returns>
        private static double FindStep(double deltaJd)
        {            
            int count = (int)(deltaJd / TimeSpan.FromMinutes(1).TotalDays) + 1;
            return deltaJd / count;
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
        internal static CrdsGeographical ProjectOnEarth(PointF p, double d, double mu, bool forceProjection = false)
        {            
            // Earth ellipticity, squared
            const double e2 = 0.00669454;

            // 8.334-1
            double rho1 = Sqrt(1 - e2 * Cos(ToRadians(d)) * Cos(ToRadians(d)));

            double xi = p.X;
            double eta = p.Y;

            // 8.333-9
            double eta1 = eta / rho1;

            // 8.333-10
            double zeta1_2 = 1 - xi * xi - eta1 * eta1;

            double zeta1;
            if (zeta1_2 > 0)
            {
                zeta1 = Sqrt(zeta1_2);
            }
            else if (!forceProjection)
            {
                return null;
            }
            else
            {
                zeta1 = 0;
                eta1 = eta;
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

            double tanPhi = Tan(phi1) / Sqrt(1 - e2);

            double phi = Atan(tanPhi);

            // 8.331-4
            double lambda = mu - theta;

            return new CrdsGeographical(To360(lambda + 180) - 180, ToDegrees(phi));
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

                return new[] { 
                    new PointF((float)ax, (float)ay), 
                    new PointF((float)bx, (float)by) }
                .OrderBy(i => -i.Y)
                .ToArray();
            }
        }

        /// <summary>
        /// Finds function root by bisection method
        /// </summary>
        /// <param name="func">Function to find root</param>
        /// <param name="a">Left edge of the interval</param>
        /// <param name="b">Right edge of the interval</param>
        /// <param name="eps">Tolerance</param>
        /// <returns>Function root</returns>
        private static double FindRoots(Func<double, double> func, double a, double b, double eps)
        {
            // check function has different 
            // signs on segment ends
            if (func(b) * func(a) > 0)
                return double.NaN;

            double dx;
            while (b - a > eps)
            {
                dx = (b - a) / 2;
                double c = a + dx;
                if (func(a) * func(c) < 0)
                {
                    b = c;
                }
                else
                {
                    a = c;
                }
            }
            return (a + b) / 2;
        }

        /// <summary>
        /// Calculates nearest solar eclipse (next or previous) for the provided Julian Day.
        /// </summary>
        /// <param name="jd"></param>
        /// <param name="next"></param>
        public static SolarEclipse NearestEclipse(double jd, bool next)
        {
            Date d = new Date(jd);
            double year = d.Year + (Date.JulianEphemerisDay(d) - Date.JulianDay0(d.Year)) / 365.25;
            double k = Floor((year - 2000) * 12.3685);
            bool eclipseFound;
          
            double T = k / 1236.85;
            double T2 = T * T;
            double T3 = T2 * T;
            double T4 = T3 * T;

            SolarEclipse eclipse = new SolarEclipse();

            do
            {
                // Moon's argument of latitude (mean dinstance of the Moon from its ascending node)
                double F = 160.7108 + 390.67050284 * k
                                    - 0.0016118 * T2
                                    - 0.00000227 * T3
                                    + 0.000000011 * T4;
                
                eclipseFound = Abs(Sin(ToRadians(F))) <= 0.36;

                if (eclipseFound)
                {
                    double jdMeanPhase = 2451550.09766 + 29.530588861 * k
                                                    + 0.00015437 * T2
                                                    - 0.000000150 * T3
                                                    + 0.00000000073 * T4;

                    // Sun's mean anomaly
                    double M = 2.5534 + 29.10535670 * k
                                     - 0.0000014 * T2
                                     - 0.00000011 * T3;
                    M = ToRadians(M);

                    // Moon's mean anomaly
                    double M_ = 201.5643 + 385.81693528 * k
                                         + 0.0107582 * T2
                                         + 0.00001238 * T3
                                         - 0.000000058 * T4;
                    M_ = ToRadians(M_);

                    // Mean longitude of ascending node
                    double Omega = 124.7746 - 1.56375588 * k
                                            + 0.0020672 * T2
                                            + 0.00000215 * T3;
                    Omega = ToRadians(Omega);

                    // Multiplier related to the eccentricity of the Earth orbit
                    double E = 1 - 0.002516 * T - 0.0000074 * T2;

                    double F1 = ToRadians(F - 0.02665 * Sin(Omega));
                    double A1 = ToRadians(299.77 + 0.107408 * k - 0.009173 * T2);

                    double jdMax =
                        jdMeanPhase
                        - 0.4075 * Sin(M_)
                        + 0.1721 * E * Sin(M)
                        + 0.0161 * Sin(2 * M_)
                        - 0.0097 * Sin(2 * F1)
                        + 0.0073 * E * Sin(M_ - M)
                        - 0.0050 * E * Sin(M_ + M)
                        - 0.0023 * Sin(M_ - 2 * F1)
                        + 0.0021 * E * Sin(2 * M)
                        + 0.0012 * Sin(M_ + 2 * F1)
                        + 0.0006 * E * Sin(2 * M_ + M)
                        - 0.0004 * Sin(3 * M_)
                        - 0.0003 * E * Sin(M + 2 * F1)
                        + 0.0003 * Sin(A1)
                        - 0.0002 * E * Sin(M - 2 * F1)
                        - 0.0002 * E * Sin(2 * M_ - M)
                        - 0.0002 * Sin(Omega);

                    double P =
                        0.2070 * E * Sin(M)
                        + 0.0024 * E * Sin(2 * M)
                        - 0.0392 * Sin(M_)
                        + 0.0116 * Sin(2 * M_)
                        - 0.0073 * E * Sin(M_ + M)
                        + 0.0067 * E * Sin(M_ - M)
                        + 0.0118 * Sin(2 * F1);

                    double Q = 
                        5.2207 - 0.0048 * E * Cos(M) 
                        + 0.0020 * E * Cos(2 * M) 
                        - 0.3299 * Cos(M_) 
                        - 0.0060 * E * Cos(M_ + M) 
                        + 0.0041 * E * Cos(M_ - M);

                    double W = Abs(Cos(F1));

                    double gamma = (P * Cos(F1) + Q * Sin(F1)) * (1 - 0.0048 * W);

                    double u = 0.0059
                        + 0.0046 * E * Cos(M)
                        - 0.0182 * Cos(M_)
                        + 0.0004 * Cos(2 * M_)
                        - 0.0005 * E * Cos(M + M_);

                    // no eclipse visible from the Earth surface
                    if (Abs(gamma) > 1.5433 + u)
                    {
                        eclipseFound = false;
                        if (next) k++;
                        else k--;
                        continue;
                    }

                    eclipse.U = u;
                    eclipse.Gamma = gamma;
                    eclipse.JulianDayMaximum = jdMax;

                    // non-central eclipse
                    if (Abs(gamma) > 0.9972 && Abs(gamma) < 0.9972 + Abs(u))
                    {
                        eclipse.IsNonCentral = true;
                    }

                    if (u < 0)
                    {
                        eclipse.EclipseType = SolarEclipseType.Total;
                    }
                    else if (u > 0.0047)
                    {
                        eclipse.EclipseType = SolarEclipseType.Annular;
                    }
                    else
                    {
                        double omega = 0.00464 * Sqrt(1 - gamma * gamma);
                        if (u < omega)
                        {
                            eclipse.EclipseType = SolarEclipseType.Hybrid;
                        }
                        else
                        {
                            eclipse.EclipseType = SolarEclipseType.Annular;
                        }
                    }

                    if (!eclipse.IsNonCentral && Abs(gamma) > 0.9972 && Abs(gamma) < 1.5433 + u)
                    {
                        eclipse.EclipseType = SolarEclipseType.Partial;
                        eclipse.Phase = (1.5433 + u - Abs(gamma)) / (0.5461 + 2 * u);
                    }

                    // hemisphere
                    if (gamma > 0)
                    {
                        eclipse.Regio = EclipseRegio.Northern;
                    }
                    if (gamma < 0)
                    {
                        eclipse.Regio = EclipseRegio.Southern;
                    }
                    if (Abs(gamma) < 0.1)
                    {
                        eclipse.Regio = EclipseRegio.Equatorial;
                    }
                }
                else
                {
                    if (next) k++;
                    else k--;
                }
            }
            while (!eclipseFound);

            return eclipse;
        }

        internal static Vector ProjectOnFundamentalPlane(CrdsGeographical g, double d, double mu)
        {
            const double e2 = 0.00669454;

            double phi = ToRadians(g.Latitude);
            double sinPhi = Sin(phi);

            double C = 1.0 / Sqrt(1 - e2 * sinPhi * sinPhi);
            double S = (1 - e2) * C;

            double a = 6378137.0;
            double h = g.Elevation;
            double phi_ = Atan((a * S + h) * Tan(phi) / (a * C + h));
            double rho = (a * C + h) * Cos(phi) / (a * Cos(phi_));

            double theta = mu - g.Longitude; // (b.Mu - 1.002738 * 360.0 / 86400.0 * deltaT) - g.Longitude;

            double xi = rho * Cos(phi_) * Sin(ToRadians(theta));
            double eta = rho * Sin(phi_) * Cos(ToRadians(d)) - rho * Cos(phi_) * Sin(ToRadians(d)) * Cos(ToRadians(theta));
            double zeta = rho * Sin(phi_) * Sin(ToRadians(d)) + rho * Cos(phi_) * Cos(ToRadians(d)) * Cos(ToRadians(theta));

            return new Vector(xi, eta, zeta);
        }

        public static bool LocalCircumstances_IsTotal(PolynomialBesselianElements pbe, CrdsGeographical g)
        {
            double step = TimeSpan.FromSeconds(1).TotalDays;

            // left edge of time interval
            double jdFrom = pbe.From;

            // right edge of time interval
            double jdTo = pbe.To - step;

            // precision of calculation, in days
            double epsilon = 1e-8;

            Func<double, double> funcLocalMax = (jd) =>
            {
                double[] dist = new double[2];

                for (int i=0; i<2; i++)
                {
                    InstantBesselianElements b = pbe.GetInstantBesselianElements(jd + i * step);
                    Vector p = ProjectOnFundamentalPlane(g, b.D, b.Mu);
                    dist[i] = Sqrt((p.X - b.X) * (p.X - b.X) + (p.Y - b.Y) * (p.Y - b.Y));
                }
                return (dist[1] - dist[0]) / step;
            };


            double jdLocalMax = FindRoots(funcLocalMax, jdFrom, jdTo, epsilon);
            
            if (!double.IsNaN(jdLocalMax))
            {
                InstantBesselianElements b = pbe.GetInstantBesselianElements(jdLocalMax);
                Vector p = ProjectOnFundamentalPlane(g, b.D, b.Mu);

                double L2zeta = b.L1 - p.Z * Tan(ToRadians(b.F1));
                
                double dist = Sqrt((p.X - b.X) * (p.X - b.X) + (p.Y - b.Y) * (p.Y - b.Y));
                if (p.Z >=0 && dist < Abs(L2zeta))
                {
                    //System.Diagnostics.Debug.WriteLine(dist);
                    return true;
                }
            }

            return false;
        }

    }
   
    /// <summary>
    /// Describes general details of the Solar eclipse
    /// </summary>
    public class SolarEclipse
    {
        /// <summary>
        /// Instant of maximal eclipse
        /// </summary>
        public double JulianDayMaximum { get; set; }

        /// <summary>
        /// Eclipse phase
        /// </summary>
        public double Phase { get; set; } = 1;
        
        /// <summary>
        /// Regio where the eclipse is primarily visible
        /// </summary>
        public EclipseRegio Regio { get; set; }        
               
        /// <summary>
        /// Least distance from the axis of the Moon's shadow to the center of the Earth,
        /// in units of equatorial radius of the Earth.
        /// </summary>
        public double Gamma { get; set; }

        /// <summary>
        /// Radius of the Moon's umbral cone in the fundamental plane,
        /// in units of equatorial radius of the Earth.
        /// </summary>
        public double U { get; set; }

        /// <summary>
        /// Type of eclipse: annular, central, hybrid (annular-central) or partial
        /// </summary>
        public SolarEclipseType EclipseType { get; set; }

        /// <summary>
        /// Flag indicating the eclipse is non-central
        /// (umbral cone touches the Earth polar regio but umbral axis does not)
        /// </summary>
        public bool IsNonCentral { get; set; }
    } 

    /// <summary>
    /// Solar eclipse type
    /// </summary>
    public enum SolarEclipseType
    {
        /// <summary>
        /// Annular solar eclipse
        /// </summary>
        Annular,

        /// <summary>
        /// Total solar eclipse
        /// </summary>
        Total,

        /// <summary>
        /// Hybrid, or annular-total solar eclipse
        /// </summary>
        Hybrid,

        /// <summary>
        /// Partial solar eclipse
        /// </summary>
        Partial
    }

    /// <summary>
    /// Visibility regio of the eclipse
    /// </summary>
    public enum EclipseRegio
    {
        /// <summary>
        /// Eclipse is primarily visible in Northern hemisphere
        /// </summary>
        Northern = 1,

        /// <summary>
        /// Eclipse is primarily visible in equatorial regio
        /// </summary>
        Equatorial = 0,

        /// <summary>
        /// Eclipse is primarily visible in Southern hemisphere
        /// </summary>
        Southern = -1
    }
}
