using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static System.Math;
using static Astrarium.Algorithms.Angle;

namespace Astrarium.Algorithms
{
    /// <summary>
    /// Contains methods for calculating lunar eclpses
    /// </summary>
    public static class LunarEclipses
    {
        /// <summary>
        /// Calculates saros series number for the lunar eclipse.
        /// </summary>
        /// <param name="jd">Julian day of the eclipse instant.</param>
        /// <returns>Saros series number for the lunar eclipse.</returns>
        /// <remarks>
        /// The method is based on algorithm described here: https://www.aa.quae.nl/en/saros.html#2_1
        /// The algorithm returns values in range from 1 to 223, 
        /// so it is not valid for some historical dates where saros series number is non-positive (see list of saros cycles here: https://en.wikipedia.org/wiki/List_of_saros_series_for_lunar_eclipses).
        /// By same reason it is not valid for further eclipse cycles with saros series number larger than 223.
        /// </remarks>
        public static int Saros(double jd)
        {
            // Full moon 18 Jan 2003
            const double jd0 = 2452656.94931;
            int LN = (int)Round((jd - jd0) / LunarEphem.SINODIC_PERIOD);
            int SNL = (192 + LN * 38 - 1) % 223 + 1;
            if (SNL < 0) SNL += 223;
            return SNL;
        }

        /// <summary>
        /// Calculates nearest lunar eclipse (next or previous) for the provided Julian Day.
        /// </summary>
        /// <param name="jd">Julian day of interest, the nearest lunar eclipse for that date will be found.</param>
        /// <param name="next">Flag indicating searching direction. True means searching next eclipse, false means previous.</param>
        public static LunarEclipse NearestEclipse(int lunationNumber, bool next)
        {
            //Date d = new Date(jd);
            //double year = d.Year + (Date.JulianEphemerisDay(d) - Date.JulianDay0(d.Year)) / 365.25;
            //double k = Floor((year - 2000) * 12.3685) + 0.5;

            //double k = LunarEphem.Lunation(jd, LunationSystem.Meeus) + 0.5;
            double k = lunationNumber + 0.5;

            bool eclipseFound;

            double T = k / 1236.85;
            double T2 = T * T;
            double T3 = T2 * T;
            double T4 = T3 * T;

            LunarEclipse eclipse = new LunarEclipse();

            do
            {
                // Moon's argument of latitude (mean dinstance of the Moon from its ascending node)
                double F = 160.7108 + 390.67050284 * k
                                    - 0.0016118 * T2
                                    - 0.00000227 * T3
                                    + 0.000000011 * T4;

                F = To360(F);
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
                    M = To360(M);
                    M = ToRadians(M);

                    // Moon's mean anomaly
                    double M_ = 201.5643 + 385.81693528 * k
                                         + 0.0107582 * T2
                                         + 0.00001238 * T3
                                         - 0.000000058 * T4;
                    M_ = To360(M_);
                    M_ = ToRadians(M_);

                    // Mean longitude of ascending node
                    double Omega = 124.7746 - 1.56375588 * k
                                            + 0.0020672 * T2
                                            + 0.00000215 * T3;

                    Omega = To360(Omega);
                    Omega = ToRadians(Omega);

                    // Multiplier related to the eccentricity of the Earth orbit
                    double E = 1 - 0.002516 * T - 0.0000074 * T2;

                    double F1 = ToRadians(F - 0.02665 * Sin(Omega));
                    double A1 = ToRadians(To360(299.77 + 0.107408 * k - 0.009173 * T2));

                    double jdMax =
                        jdMeanPhase
                        - 0.4065 * Sin(M_)
                        + 0.1727 * E * Sin(M)
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

                    double rho = 1.2848 + u;
                    double sigma = 0.7403 - u;

                    double mag = (1.0128 - u - Abs(gamma)) / 0.5450;

                    if (mag >= 1)
                    {
                        eclipse.EclipseType = LunarEclipseType.Total;
                    }
                    if (mag > 0 && mag < 1)
                    {
                        eclipse.EclipseType = LunarEclipseType.Partial;
                    }

                    // Check if elipse is penumbral only
                    if (mag < 0)
                    {
                        eclipse.EclipseType = LunarEclipseType.Penumbral;
                        mag = (1.5573 + u - Abs(gamma)) / 0.5450;
                    }

                    // No eclipse, if both phases is less than 0.
                    // Examine other lunation 
                    if (mag < 0)
                    {
                        eclipseFound = false;
                    }
                    // Eclipse found
                    else
                    {
                        eclipse.JulianDayMaximum = jdMax;
                        eclipse.Magnitude = mag;
                        eclipse.Rho = rho;
                        eclipse.Gamma = gamma;
                        eclipse.Sigma = sigma;

                        double p = 1.0128 - u;
                        double t = 0.4678 - u;
                        double n = 1.0 / (24 * (0.5458 + 0.04 * Cos(M_)));
                        double h = 1.5573 + u;

                        double sdPartial = n * Sqrt(p * p - gamma * gamma);
                        double sdTotal = n * Sqrt(t * t - gamma * gamma);
                        double sdPenumbra = n * Sqrt(h * h - gamma * gamma);

                        eclipse.JulianDayFirstContactPenumbra = jdMax - sdPenumbra;
                        eclipse.JulianDayFirstContactUmbra = jdMax - sdPartial;
                        eclipse.JulianDayTotalBegin = jdMax - sdTotal;
                        eclipse.JulianDayTotalEnd = jdMax + sdTotal;
                        eclipse.JulianDayLastContactUmbra = jdMax + sdPartial;
                        eclipse.JulianDayLastContactPenumbra = jdMax + sdPenumbra;
                        eclipse.Saros = Saros(jdMax);
                        eclipse.MeeusLunationNumber = (int)Round(k - 0.5);
                    }
                }

                if (!eclipseFound)
                {
                    if (next) k++;
                    else k--;
                }
            }
            while (!eclipseFound);

            return eclipse;
        }

        public static PolynomialLunarEclipseElements BesselianElements(double jdMaximum, SunMoonPosition[] positions)
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
            InstantLunarEclipseElements[] elements = new InstantLunarEclipseElements[5];

            PointF[] points = new PointF[5];
            for (int i = 0; i < 5; i++)
            {
                elements[i] = BesselianElements(positions[i]);
                points[i].X = i - 2;
            }

            // Alpha expressed in degrees and can cross zero point (360 -> 0).
            // Values must be aligned in order to avoid crossing.
            double[] Alpha = Align(elements.Select(e => e.Alpha).ToArray());

            return new PolynomialLunarEclipseElements()
            {
                JulianDay0 = positions[2].JulianDay,
                JulianDayMaximum = jdMaximum,
                DeltaT = Date.DeltaT(positions[2].JulianDay),
                Step = step,
                X = LeastSquares.FindCoeffs(points.Select((p, i) => new PointF(p.X, (float)elements[i].X)), 3),
                Y = LeastSquares.FindCoeffs(points.Select((p, i) => new PointF(p.X, (float)elements[i].Y)), 3),
                F1 = LeastSquares.FindCoeffs(points.Select((p, i) => new PointF(p.X, (float)elements[i].F1)), 3),
                F2 = LeastSquares.FindCoeffs(points.Select((p, i) => new PointF(p.X, (float)elements[i].F2)), 3),
                F3 = LeastSquares.FindCoeffs(points.Select((p, i) => new PointF(p.X, (float)elements[i].F3)), 3),
                Alpha = LeastSquares.FindCoeffs(points.Select((p, i) => new PointF(p.X, (float)Alpha[i])), 3),
                Delta = LeastSquares.FindCoeffs(points.Select((p, i) => new PointF(p.X, (float)elements[i].Delta)), 3),
            };
        }

        /// <summary>
        /// Calculates Besselian elements for lunar eclipse,
        /// valid only for specified instant.
        /// </summary>
        /// <param name="position">Sun and Moon position data</param>
        /// <returns>
        /// Besselian elements for lunar eclipse
        /// </returns>
        /// <remarks>
        /// The method is based on formulae given here:
        /// https://de.wikipedia.org/wiki/Besselsche_Elemente
        /// </remarks>
        internal static InstantLunarEclipseElements BesselianElements(SunMoonPosition position)
        {
            // Geocentric RA of the center of Earth shadow
            double a = ToRadians(position.Sun.Alpha + 180);

            // Geocentric Dec of the center of Earth shadow
            double d = ToRadians(-position.Sun.Delta);

            // Geocentric RA of the Moon, in radians
            double am = ToRadians(position.Moon.Alpha);

            // Geocentric Dec of the Moon, in radians
            double dm = ToRadians(position.Moon.Delta);
           
            double x = Cos(dm) * Sin(am - a);
            double y = Sin(dm) * Cos(d) - Cos(dm) * Sin(d) * Cos(am - a);

            // Astronomical unit, in km
            const double AU = 149597870;

            // Earth radius, in km
            const double EARTH_RADIUS = 6371;

            // Seconds in degree
            const double SEC_IN_DEGREE = 3600;

            // Geocentric solar radius, in degrees
            double rs = SolarEphem.Semidiameter(position.DistanceSun * EARTH_RADIUS / AU) / SEC_IN_DEGREE;

            // Geocentric lunar radius, in degrees
            double rm = LunarEphem.Semidiameter(position.DistanceMoon * EARTH_RADIUS) / SEC_IN_DEGREE;

            // Geocentric parallax of the Sun, in degrees
            double pi_s = LunarEphem.Parallax(position.DistanceSun * EARTH_RADIUS);

            // Geocentric parallax of the Moon, in degrees
            double pi_m = LunarEphem.Parallax(position.DistanceMoon * EARTH_RADIUS);

            // Danjon rule of shadow enlargement is used
            const double ENLARGEMENT = 1.01;

            // Earth penumbra radius, in degrees
            double f1 = ENLARGEMENT * pi_m + pi_s + rs;

            // Earth umbra radius, in degrees
            double f2 = ENLARGEMENT * pi_m + pi_s - rs;

            // Lunar radius (semidiameter), in degrees
            double f3 = rm;

            return new InstantLunarEclipseElements()
            {
                JulianDay = position.JulianDay,
                // Convert to degrees: 
                // http://www.eclipsewise.com/lunar/LEprime/2001-2100/LE2021May26Tprime.html
                X = ToDegrees(x),
                Y = ToDegrees(y),
                F1 = f1,
                F2 = f2,
                F3 = f3,
                Alpha = position.Moon.Alpha,
                Delta = position.Moon.Delta
            };
        }

        /// <summary>
        /// Calculates lunar eclipse map
        /// </summary>
        /// <param name="eclipse">Eclipse general details</param>
        /// <param name="elements">Besselian elements for the eclipse</param>
        /// <returns>Eclipse map</returns>
        public static LunarEclipseMap EclipseMap(LunarEclipse eclipse, PolynomialLunarEclipseElements elements)
        {
            return new LunarEclipseMap()
            {
                PenumbralBegin = !double.IsNaN(eclipse.JulianDayFirstContactPenumbra) ? FindCurvePoints(elements.GetInstantBesselianElements(eclipse.JulianDayFirstContactPenumbra)) : null,
                PartialBegin = !double.IsNaN(eclipse.JulianDayFirstContactUmbra) ? FindCurvePoints(elements.GetInstantBesselianElements(eclipse.JulianDayFirstContactUmbra)) : null,
                TotalBegin = !double.IsNaN(eclipse.JulianDayTotalBegin) ? FindCurvePoints(elements.GetInstantBesselianElements(eclipse.JulianDayTotalBegin)) : null,
                TotalEnd = !double.IsNaN(eclipse.JulianDayTotalEnd) ? FindCurvePoints(elements.GetInstantBesselianElements(eclipse.JulianDayTotalEnd)) : null,
                PartialEnd = !double.IsNaN(eclipse.JulianDayLastContactUmbra) ? FindCurvePoints(elements.GetInstantBesselianElements(eclipse.JulianDayLastContactUmbra)) : null,
                PenumbralEnd = !double.IsNaN(eclipse.JulianDayLastContactPenumbra) ? FindCurvePoints(elements.GetInstantBesselianElements(eclipse.JulianDayLastContactPenumbra)) : null,
            };
        }

        private static IList<CrdsGeographical> FindCurvePoints(InstantLunarEclipseElements e)
        {
            var curve = new List<CrdsGeographical>();
           
            var p0 = Project(e, -180);
            curve.Add(new CrdsGeographical(p0.Longitude, -88 * Sign(e.Delta)));

            for (int i = -180; i <= 180; i++)
            {
                var p = Project(e, i);
                curve.Add(p);
            }

            var p1 = Project(e, 180);
            curve.Add(new CrdsGeographical(p1.Longitude, -88 * Sign(e.Delta)));

            return curve;
        }

        /// <summary>
        /// Finds horizon circle point for a given geographical longitude 
        /// for an instant of lunar eclipse, where Moon has zero altitude.
        /// </summary>
        /// <param name="e">Besselian elements of the Moon for the given instant.</param>
        /// <param name="L">Geographical longitude, positive west, negative east, from -180 to +180 degrees.</param>
        /// <returns>
        /// Geographical coordinates for the point.
        /// </returns>
        /// <remarks>
        /// The method core is based on formulae from the book:
        /// Seidelmann, P. K.: Explanatory Supplement to The Astronomical Almanac, 
        /// University Science Book, Mill Valley (California), 1992,
        /// Chapter 8 "Eclipses of the Sun and Moon"
        /// https://archive.org/download/131123ExplanatorySupplementAstronomicalAlmanac/131123-explanatory-supplement-astronomical-almanac.pdf
        /// </remarks>
        private static CrdsGeographical Project(InstantLunarEclipseElements e, double L)
        {
            CrdsGeographical g = null;
            
            // Nutation elements
            var nutation = Nutation.NutationElements(e.JulianDay);

            // True obliquity
            var epsilon = Date.TrueObliquity(e.JulianDay, nutation.deltaEpsilon);

            // Greenwich apparent sidereal time 
            double siderealTime = Date.ApparentSiderealTime(e.JulianDay, nutation.deltaPsi, epsilon);

            // Geocenric distance to the Moon, in km
            double dist = 358473400.0 / (e.F3 * 3600);

            // Horizontal parallax of the Moon
            double parallax = LunarEphem.Parallax(dist);

            // Equatorial coordinates of the Moon, initial value is geocentric
            CrdsEquatorial eq = new CrdsEquatorial(e.Alpha, e.Delta);

            // two iterations:
            // 1st: find geo location needed to perform topocentric correction
            // 2nd: correct sublunar point with topocentric position and find true geoposition
            for (int i = 0; i < 2; i++)
            {
                // sublunar point latitude, preserve sign!
                double phi0 = Sign(e.Delta) * Abs(eq.Delta);

                // sublunar point longitude (formula 8.426-1)
                double lambda0 = siderealTime - eq.Alpha;

                // sublunar point latitude (formula 8.426-2)
                double tanPhi = -1.0 / Tan(ToRadians(phi0)) * Cos(ToRadians(lambda0 - L));
                double phi = ToDegrees(Atan(tanPhi));

                g = new CrdsGeographical(L, phi);

                if (i == 0)
                {
                    // correct to topocentric
                    eq = eq.ToTopocentric(g, siderealTime, parallax);
                }
            }

            return g;
        }

        public static LunarEclipseLocalCircumstances LocalCircumstances(LunarEclipse eclipse, PolynomialLunarEclipseElements e, CrdsGeographical g)
        {           
            return new LunarEclipseLocalCircumstances()
            {
                Location = g,
                PenumbralBegin = GetLocalCircumstancesContactPoint(eclipse.JulianDayFirstContactPenumbra, eclipse, e, g),
                PartialBegin = GetLocalCircumstancesContactPoint(eclipse.JulianDayFirstContactUmbra, eclipse, e, g),
                TotalBegin = GetLocalCircumstancesContactPoint(eclipse.JulianDayTotalBegin, eclipse, e, g),
                Maximum = GetLocalCircumstancesContactPoint(eclipse.JulianDayMaximum, eclipse, e, g),
                TotalEnd = GetLocalCircumstancesContactPoint(eclipse.JulianDayTotalEnd, eclipse, e, g),
                PartialEnd = GetLocalCircumstancesContactPoint(eclipse.JulianDayLastContactUmbra, eclipse, e, g),
                PenumbralEnd = GetLocalCircumstancesContactPoint(eclipse.JulianDayLastContactPenumbra, eclipse, e, g),
            };
        }

        private static LunarEclipseLocalCircumstancesContactPoint GetLocalCircumstancesContactPoint(double jd, LunarEclipse eclipse, PolynomialLunarEclipseElements e, CrdsGeographical g)
        {
            if (!double.IsNaN(jd))
                return new LunarEclipseLocalCircumstancesContactPoint(e.GetInstantBesselianElements(jd), g);
            else
                return null;
        }
    }
}
