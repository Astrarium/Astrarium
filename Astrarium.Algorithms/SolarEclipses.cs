using System;
using System.Collections.Generic;
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
        /// Calculates saros series number for the solar eclipse.
        /// </summary>
        /// <param name="jd">Julian day of the eclipse instant.</param>
        /// <returns>Saros series number for the solar eclipse.</returns>
        /// <remarks>
        /// The method is based on algorithm described here: https://webspace.science.uu.nl/~gent0113/eclipse/eclipsecycles.htm
        /// </remarks>
        public static int Saros(double jd)
        {
            int LN = LunarEphem.Lunation(jd, LunationSystem.Meeus);
            int ND = LN + 105;
            int NS = 136 + 38 * ND;
            int NX = -61 * ND;
            int NC = (int)Floor(NX / 358.0 + 0.5 - ND / (12.0 * 358 * 358));
            int SNS = (NS + NC * 223 - 1) % 223 + 1;
            return SNS;
        }

        /// <summary>
        /// Calculates nearest solar eclipse (next or previous) for the provided Julian Day.
        /// </summary>
        /// <param name="jd">Julian day of interest, the nearest solar eclipse for that date will be found.</param>
        /// <param name="next">Flag indicating searching direction. True means searching next eclipse, false means previous.</param>
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
                        eclipse.Magnitude = (1.5433 + u - Abs(gamma)) / (0.5461 + 2 * u);
                    }
                   
                    // saros
                    eclipse.Saros = Saros(jdMax);
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

        /// <summary>
        /// Calculates local circumstances of solar eclipse.
        /// </summary>
        /// <param name="pbe"><see cref="PolynomialBesselianElements"/> Besselian elements for the eclipse</param>
        /// <param name="g">Geographical coordinates to get eclipse circumstances for.</param>
        /// <returns><see cref="SolarEclipseLocalCircumstances"/> instance containing local curcumstances details.</returns>
        /// <remarks>
        /// The method is from book J.Meeus "Elements of Solar Eclipses", 1951-2000, § 23.
        /// </remarks>
        public static SolarEclipseLocalCircumstances LocalCircumstances(PolynomialBesselianElements pbe, CrdsGeographical g)
        {
            double deltaT = pbe.DeltaT;
            double phi = ToRadians(g.Latitude);
            double t = 0;
            double tau = 0;
            int iters = 0;

            // Earth flattening
            const double flat = 1.0 / 298.257;

            // Earth flattening constant, used in calculation: 0.99664719
            double fconst = 1.0 - flat;
            double U = Atan(fconst * Tan(phi));
            double rhoSinPhi_ = fconst * Sin(U);
            double rhoCosPhi_ = Cos(U);

            double jd;

            double d, H, h, ksi, eta, zeta, u, v, a, b, n2, n, S, X, Y;
            InstantBesselianElements be;

            do
            {
                iters++;

                jd = pbe.JulianDay0 + t * pbe.Step;
                be = pbe.GetInstantBesselianElements(jd);

                X = be.X;
                Y = be.Y;
                d = ToRadians(be.D);
                double M = be.Mu;
                double dX = be.dX;
                double dY = be.dY;

                H = ToRadians(M - g.Longitude - 0.00417807 * deltaT);

                ksi = rhoCosPhi_ * Sin(H);
                eta = rhoSinPhi_ * Cos(d) - rhoCosPhi_ * Cos(H) * Sin(d);
                zeta = rhoSinPhi_ * Sin(d) + rhoCosPhi_ * Cos(H) * Cos(d);

                double sinh = Sin(d) * Sin(phi) + Cos(d) * Cos(phi) * Cos(H);
                h = ToDegrees(Asin(sinh));

                double ksi_ = ToRadians(pbe.Mu[1] * rhoCosPhi_ * Cos(H));
                double eta_ = ToRadians(pbe.Mu[1] * ksi * Sin(d) - zeta * pbe.D[1]);

                u = X - ksi;
                v = Y - eta;

                a = dX - ksi_;
                b = dY - eta_;

                n2 = a * a + b * b;
                n = Sqrt(n2);

                tau = -(u * a + v * b) / n2;
                t += tau;
            }
            while (Abs(tau) >= 0.00001 && iters < 20);

            double tMax = t;
            double jdMax = jd;
            double altMax = h;
           
            double dL1 = be.L1 - zeta * pbe.TanF1;
            double dL2 = be.L2 - zeta * pbe.TanF2;

            // calculate instantaneous magnitude (EOSE, p. 29)
            double m = Sqrt(u * u + v * v);
            double G = (dL1 - m) / (dL1 + dL2);
            double A = (dL1 - dL2) / (dL1 + dL2);

            // calculate path width (EOSE, p. 12)
            double omega = 1.0 / Sqrt(1 - 0.006_694_385 * Cos(d) * Cos(d));
            double Y1 = omega * Y;
            double B = Sqrt(1 - X * X - Y1 * Y1);
            double K2 = B * B + (X * a + Y * b) * (X * a + Y * b) / n2;
            double width = 12756 * Abs(dL2) / Sqrt(K2);

            // calculate position angles (EOSE, pp. 26-27)
            double P = Atan2(u, v);
            double q = Asin(Cos(phi) * Sin(H) / Cos(ToRadians(h)));
            double zenithAngleMax = To360(ToDegrees(P - q));
            double posAngleMax = To360(ToDegrees(P));

            if (G < 0)
            {
                return new SolarEclipseLocalCircumstances() { IsInvisible = true };
            }

            var tauPhases = new double[4];
            var jdPhases = new double[4];
            var altPhases = new double[4];
            var zenithAngles = new double[4];
            var posAngles = new double[4];

            // calculate initial values of tau
            for (int i = 0; i < 4; i++)
            {
                double tanFx = i < 2 ? pbe.TanF1 : pbe.TanF2;
                double Lx = i < 2 ? be.L1 : be.L2;
                int sign = i % 2 == 0 ? -1 : 1;
                double dLx = Lx - zeta * tanFx;
                S = (a * v - u * b) / (n * dLx);
                tauPhases[i] = -(u * a + v * b) / n2 + sign * dLx / n * Sqrt(1 - S * S);
            }

            // i=0: beginning of partial phase
            // i=1: end of partial phase
            // i=2: beginning of total phase
            // i=3: end of total phase
            for (int i = 0; i < 4; i++)
            {
                t = tMax + tauPhases[i];
                iters = 0;

                do
                {
                    iters++;

                    jd = pbe.JulianDay0 + t * pbe.Step;
                    be = pbe.GetInstantBesselianElements(jd);

                    X = be.X;
                    Y = be.Y;
                    d = ToRadians(be.D);
                    double M = be.Mu;
                    double dX = be.dX;
                    double dY = be.dY;

                    H = ToRadians(M - g.Longitude - 0.00417807 * deltaT);

                    double sinh = Sin(d) * Sin(phi) + Cos(d) * Cos(phi) * Cos(H);
                    h = ToDegrees(Asin(sinh));

                    ksi = rhoCosPhi_ * Sin(H);
                    eta = rhoSinPhi_ * Cos(d) - rhoCosPhi_ * Cos(H) * Sin(d);
                    zeta = rhoSinPhi_ * Sin(d) + rhoCosPhi_ * Cos(H) * Cos(d);

                    double tanFx = i < 2 ? pbe.TanF1 : pbe.TanF2;
                    double Lx = i < 2 ? be.L1 : be.L2;
                    int sign = i % 2 == 0 ? -1 : 1;
                    double dLx = Lx - zeta * tanFx;

                    double ksi_ = ToRadians(pbe.Mu[1] * rhoCosPhi_ * Cos(H));
                    double eta_ = ToRadians(pbe.Mu[1] * ksi * Sin(d) - zeta * pbe.D[1]);

                    u = X - ksi;
                    v = Y - eta;

                    a = dX - ksi_;
                    b = dY - eta_;

                    n2 = a * a + b * b;
                    n = Sqrt(n2);

                    S = (a * v - u * b) / (n * dLx);
                    tau = -(u * a + v * b) / n2 + sign * dLx / n * Sqrt(1 - S * S);
                    t += tau;
                }
                while (Abs(tau) >= 0.00001 && iters < 20);

                P = Atan2(u, v);
                q = Asin(Cos(phi) * Sin(H) / Cos(ToRadians(h)));
                posAngles[i] = To360(ToDegrees(P));
                zenithAngles[i] = To360(ToDegrees(P - q));
                jdPhases[i] = jd;
                altPhases[i] = h;
            }

            if (double.IsNaN(width) || double.IsNaN(jdPhases[2]) || double.IsNaN(jdPhases[3]))
            {
                width = 0;
            }

            if (altPhases[0] < 0 && altPhases[1] < 0)
            {
                return new SolarEclipseLocalCircumstances() { IsInvisible = true };
            }
            else
            {
                return new SolarEclipseLocalCircumstances()
                {
                    JulianDayMax = jdMax,
                    SunAltMax = altMax,
                    ZAngleMax = zenithAngleMax, 
                    PAngleMax = posAngleMax,
                    JulianDayPartialBegin = jdPhases[0],
                    SunAltPartialBegin = altPhases[0],
                    ZAnglePartialBegin = zenithAngles[0],
                    PAnglePartialBegin = posAngles[0],
                    JulianDayPartialEnd = jdPhases[1],
                    SunAltPartialEnd = altPhases[1],
                    ZAnglePartialEnd = zenithAngles[1],
                    PAnglePartialEnd = posAngles[1],
                    JulianDayTotalBegin = Min(jdPhases[2], jdPhases[3]),
                    SunAltTotalBegin = altPhases[2],
                    ZAngleTotalBegin = zenithAngles[2],
                    PAngleTotalBegin = posAngles[2],
                    JulianDayTotalEnd = Max(jdPhases[2], jdPhases[3]),
                    SunAltTotalEnd = altPhases[3],
                    ZAngleTotalEnd = zenithAngles[3],
                    PAngleTotalEnd = posAngles[3],
                    MaxMagnitude = G,
                    MoonToSunDiameterRatio = A,
                    PathWidth = width
                };
            }
        }

        /// <summary>
        /// Finds polynomial Besselian elements by 5 positions of Sun and Moon.
        /// </summary>
        /// <param name="jdMaximum">Instant of eclipse maximum.</param>
        /// <param name="positions">Positions of Sun and Moon.</param>
        /// <returns>Polynomial Besselian elements of the Solar eclipse.</returns>
        public static PolynomialBesselianElements BesselianElements(double jdMaximum, SunMoonPosition[] positions)
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
                elements[i] = BesselianElements(positions[i]);
                points[i].X = i - 2;
            }

            // Mu expressed in degrees and can cross zero point.
            // Values must be aligned in order to avoid crossing.
            double[] Mu = elements.Select(e => e.Mu).ToArray();
            Align(Mu);
            for (int i = 0; i < 5; i++)
            {
                elements[i].Mu = Mu[i];      
            }

            return new PolynomialBesselianElements()
            {
                JulianDay0 = positions[2].JulianDay,
                JulianDayMaximum = jdMaximum,
                DeltaT = Date.DeltaT(positions[2].JulianDay),
                Step = step,
                X = LeastSquares.FindCoeffs(points.Select((p, i) => new PointF(p.X, (float)elements[i].X)), 3),
                Y = LeastSquares.FindCoeffs(points.Select((p, i) => new PointF(p.X, (float)elements[i].Y)), 3),
                L1 = LeastSquares.FindCoeffs(points.Select((p, i) => new PointF(p.X, (float)elements[i].L1)), 3),
                L2 = LeastSquares.FindCoeffs(points.Select((p, i) => new PointF(p.X, (float)elements[i].L2)), 3),
                D = LeastSquares.FindCoeffs(points.Select((p, i) => new PointF(p.X, (float)elements[i].D)), 3),
                Mu = LeastSquares.FindCoeffs(points.Select((p, i) => new PointF(p.X, (float)elements[i].Mu)), 3),
                TanF1 = elements.Average(p => p.TanF1),
                TanF2 = elements.Average(p => p.TanF2),
            };
        }

        /// <summary>
        /// Gets map of solar eclipse.
        /// </summary>
        /// <param name="pbe">Polynomial Besselian elements defining the Eclipse</param>
        /// <returns><see cref="SolarEclipseMap"/> instance.</returns>
        public static SolarEclipseMap EclipseMap(PolynomialBesselianElements pbe)
        {
            // left edge of time interval
            double jdFrom = pbe.From;

            // midpoint of time interval
            double jdMid = pbe.From + (pbe.To - pbe.From) / 2;

            // right edge of time interval
            double jdTo = pbe.To;

            // precision of calculation, in days
            double epsilon = 1e-8;

            // Eclipse general details
            SolarEclipse eclipse = NearestEclipse(pbe.JulianDay0, true);

            // Eclipse map data
            SolarEclipseMap map = new SolarEclipseMap();

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

            // Instant of first external contact of penumbra,
            // assume always exists
            double jdP1 = FindRoots(funcExternalContact, jdFrom, jdMid, epsilon);
            {
                InstantBesselianElements b = pbe.GetInstantBesselianElements(jdP1);
                double a = Atan2(b.Y, b.X);
                PointF p = new PointF((float)Cos(a), (float)Sin(a));
                map.P1 = new SolarEclipseMapPoint(jdP1, Project(b, p));
            }

            // Instant of last external contact of penumbra
            // assume always exists
            double jdP4 = FindRoots(funcExternalContact, jdMid, jdTo, epsilon);
            {
                InstantBesselianElements b = pbe.GetInstantBesselianElements(jdP4);
                double a = Atan2(b.Y, b.X);
                PointF p = new PointF((float)Cos(a), (float)Sin(a));
                map.P4 = new SolarEclipseMapPoint(jdP4, Project(b, p));
            }

            // Instant of first internal contact of penumbra,
            // may not exist
            double jdP2 = FindRoots(funcInternalContact, jdFrom, jdMid, epsilon);
            if (!double.IsNaN(jdP2))
            {
                InstantBesselianElements b = pbe.GetInstantBesselianElements(jdP2);
                double a = Atan2(b.Y, b.X);
                PointF p = new PointF((float)Cos(a), (float)Sin(a));
                map.P2 = new SolarEclipseMapPoint(jdP2, Project(b, p));
            }

            // Instant of last internal contact of penumbra,
            // may not exist
            double jdP3 = FindRoots(funcInternalContact, jdMid, jdTo, epsilon);
            if (!double.IsNaN(jdP3))
            {
                InstantBesselianElements b = pbe.GetInstantBesselianElements(jdP3);
                double a = Atan2(b.Y, b.X);
                PointF p = new PointF((float)Cos(a), (float)Sin(a));
                map.P3 = new SolarEclipseMapPoint(jdP3, Project(b, p));
            }

            // There are no C1/C2 points for partial and non-central eclipses
            if (eclipse.EclipseType != SolarEclipseType.Partial && !eclipse.IsNonCentral)
            {
                map.C1 = FindExtremeLimitOfCentralLine(pbe, true);
                map.C2 = FindExtremeLimitOfCentralLine(pbe, false);
            }

            // Instant of eclipse maximum
            {
                InstantBesselianElements b = pbe.GetInstantBesselianElements(pbe.JulianDay0);
                PointF p = new PointF((float)b.X, (float)b.Y);
                var g = Project(b, p);
                if (g != null)
                {
                    map.Max = new SolarEclipseMapPoint(pbe.JulianDayMaximum, g);
                }
            }

            if (eclipse.EclipseType != SolarEclipseType.Partial)
            {
                map.TotalPath = FindCurvePoints(pbe, 0, 0);
                var northPoints = FindCurvePoints(pbe, 1, 1);
                var southPoints = FindCurvePoints(pbe, -1, 1);

                if (northPoints.Count > 2)
                {
                    var g = northPoints.OrderByDescending(p => Abs(p.Latitude)).First();
                    int index = northPoints.IndexOf(g);
                    map.UmbraNorthernLimit[0] = northPoints.Take(index).ToList();
                    map.UmbraNorthernLimit[1] = northPoints.Skip(index - 1).ToList();
                }

                if (southPoints.Count > 2)
                {
                    var g = southPoints.OrderByDescending(p => Abs(p.Latitude)).First();
                    int index = southPoints.IndexOf(g);
                    map.UmbraSouthernLimit[0] = southPoints.Take(index).ToList();
                    map.UmbraSouthernLimit[1] = southPoints.Skip(index - 1).ToList();
                }
            }

            // Find points of Northern limit of penumbra
            map.PenumbraNorthernLimit = FindCurvePoints(pbe, 1, 0);
            map.PenumbraSouthernLimit = FindCurvePoints(pbe, -1, 0);

            // Calc rise/set curves
            FindRiseSetCurves(pbe, map, jdP1, jdP4);

            return map;
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
        internal static InstantBesselianElements BesselianElements(SunMoonPosition position)
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

            double tanF1 = Tan(F1);
            double tanF2 = Tan(F2);

            double l1 = zv1 * tanF1;
            double l2 = zv2 * tanF2;

            return new InstantBesselianElements()
            {
                X = x,
                Y = y,
                L1 = l1,
                L2 = l2,
                D = ToDegrees(d),
                Mu = To360(theta - ToDegrees(a)),
                TanF1 = tanF1,
                TanF2 = tanF2
            };
        }

        /// <summary>
        /// Gets point of a specified curve of solar eclipse map, for specified geographical longitude.
        /// </summary>
        /// <param name="pbe">Polynomial Besselian elements.</param>
        /// <param name="g">
        /// Geographical coordinates of a point to start iteration from.
        /// Longitude of the final point will remain unchanged, but latitude will be adjusted.
        /// </param>
        /// <param name="i">Type of the curve (see remarks)</param>
        /// <param name="G">Magnitude (see remarks)</param>
        /// <returns>
        /// Point of the eclipse's map curve, for specified longitude, if exists.
        /// </returns>
        /// <remarks>
        /// The method is from book J.Meeus "Elements of Solar Eclipses, 1951-2000, § 22.
        /// <paramref name="i"/> and <paramref name="G"/> define a curve type:
        /// <code>
        /// ------------------------------------------------------------------------------
        /// Type of curve                                          i              G
        /// ------------------------------------------------------------------------------
        /// central line                                           0         irrelevant
        /// northern limit of path of total or annular eclipse    +1             +1
        /// southern limit of path of total or annular eclipse    -1             +1
        /// northern limit of partial eclipse                     +1              0
        /// southern limit of partial eclipse                     -1              0
        /// equal magnitude (north)                               +1         the given G
        /// equal magnitude (south)                               -1         the given G
        /// -------------------------------------------------------------------------------
        /// </code>
        /// </remarks>
        internal static SolarEclipseMapPoint FindEclipseCurvePoint(PolynomialBesselianElements pbe, CrdsGeographical g, int i = 0, double G = 0)
        {
            // Sanity checks:
            if (!(i == 0 || i == -1 || i == 1))
                throw new ArgumentException($"Invalid value of {nameof(i)} argument", nameof(i));
            if (G < 0 || G > 1)
                throw new ArgumentException($"Invalid value of {nameof(G)} argument", nameof(G));

            double deltaT = pbe.DeltaT;

            double t = 0;   // time since jd0
            double phi = g.Latitude; // latitude

            double tau;
            double deltaPhi;

            int iters = 0;

            // Earth flattening
            const double flat = 1.0 / 298.257;

            // Earth flattening constant, used in calculation: 0.99664719
            double fconst = 1.0 - flat;

            double jd;

            double d, H, h;

            do
            {
                iters++;

                jd = pbe.JulianDay0 + t * pbe.Step;
                var be = pbe.GetInstantBesselianElements(jd);

                double X = be.X;
                double Y = be.Y;
                d = ToRadians(be.D);
                double M = be.Mu;
                H = ToRadians(M - g.Longitude - 0.00417807 * deltaT);

                double dX = be.dX; 
                double dY = be.dY;

                double U = Atan(fconst * Tan(ToRadians(phi)));

                double rhoSinPhi_ = fconst * Sin(U);
                double rhoCosPhi_ = Cos(U);

                double sinh = Sin(d) * Sin(ToRadians(phi)) + Cos(d) * Cos(ToRadians(phi)) * Cos(H);
                h = ToDegrees(Asin(sinh));

                double ksi = rhoCosPhi_ * Sin(H);
                double eta = rhoSinPhi_ * Cos(d) - rhoCosPhi_ * Cos(H) * Sin(d);
                double zeta = rhoSinPhi_ * Sin(d) + rhoCosPhi_ * Cos(H) * Cos(d);

                // take athmospheric refraction into account
                // J.Meeus "Elements of Solar Eclipses 1950-2200", page 29
                if (G != 0 && h >= 0 && h <= 10)
                {
                    // this is an exponential function that fits values listed in the book
                    // Function form is: y(x) = a + b * e ^ (-c*x)
                    double sigma = 1.000012 + 0.0002282559 * Exp(-0.5035747 * h);

                    ksi *= sigma;
                    eta *= sigma;
                    zeta *= sigma;
                }

                double ksi_ = ToRadians(pbe.Mu[1] * rhoCosPhi_ * Cos(H));
                double eta_ = ToRadians(pbe.Mu[1] * ksi * Sin(d) - zeta * pbe.D[1]);

                double u = X - ksi;
                double v = Y - eta;

                double a = dX - ksi_;
                double b = dY - eta_;

                double n2 = a * a + b * b;

                double n = Sqrt(n2);

                tau = -(u * a + v * b) / n2;

                double W = (v * a - u * b) / n;

                double Q1 = b * Sin(H) * rhoSinPhi_;
                double Q2 = a * (Cos(H) * Sin(d) * rhoSinPhi_ + Cos(d) * rhoCosPhi_);

                double Q = (Q1 + Q2) / ToDegrees(n);

                double dL1 = be.L1 - zeta * pbe.TanF1;
                double dL2 = be.L2 - zeta * pbe.TanF2;

                double E = dL1 - G * (dL1 + dL2);

                deltaPhi = (W + i * Abs(E)) / Q;

                t += tau;
                phi += deltaPhi;
            }
            while ((Abs(tau) >= 0.0001 || Abs(deltaPhi) >= 0.0001) && iters < 50);

            if (Abs(phi) > 90)
            {
                return null;
            }

            // Sun is below horizon
            if (h < 0)
            {
                return null;
            }
            
            if (Abs(tau) < 0.0001 && Abs(deltaPhi) < 0.0001)
            {
                return new SolarEclipseMapPoint(jd, new CrdsGeographical(g.Longitude, phi));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Finds extreme points on central line of eclipse.
        /// </summary>
        /// <param name="pbe">Polynomial Besselian elements.</param>
        /// <param name="begin">Flag indicating type of the point. True means begin point, false means end point.</param>
        /// <returns>
        /// Eclipse point C1 (begin) or C2 (end) of the central line.
        /// </returns>
        internal static SolarEclipseMapPoint FindExtremeLimitOfCentralLine(PolynomialBesselianElements pbe, bool begin)
        {
            double d = ToRadians(pbe.D[0]);
            double omega = 1.0 / Sqrt(1 - 0.006_694_385 * Cos(d) * Cos(d));

            double u = pbe.X[0];
            double v = omega * pbe.Y[0];

            double a = pbe.X[1];
            double b = omega * pbe.Y[1];

            double n2 = a * a + b * b;
            double n = Sqrt(n2);

            double S = (a * v - u * b) / n;

            double t1 = -(u * a + v * b) / n2;
            double t2 = Sqrt(1 - S * S) / n;

            double tau;
            double jd = pbe.JulianDay0;
            InstantBesselianElements be;
            do
            {
                tau = (t1 + (begin ? -1 : 1) * t2) * pbe.Step;
                jd += tau;

                be = pbe.GetInstantBesselianElements(jd);

                d = ToRadians(be.D);
                omega = 1.0 / Sqrt(1 - 0.006_694_385 * Cos(d) * Cos(d));

                u = be.X;
                v = omega * be.Y;
                a = be.dX;
                b = omega * be.dY;
                n2 = a * a + b * b;
                n = Sqrt(n2);
                
                S = (a * v - u * b) / n;
                t1 = -(u * a + v * b) / n2;
                t2 = Sqrt(1 - S * S) / n;

                tau = (t1 + (begin ? -1 : 1) * t2) * pbe.Step;
                jd += tau;
            }
            while (Abs(tau) > 0.0001);

            be = pbe.GetInstantBesselianElements(jd);
            d = ToRadians(be.D);
            double omega2 = 1.0 / (1 - 0.006_694_385 * Cos(d) * Cos(d));
            double H = To360(ToDegrees(Atan2(be.X, -omega2 * be.Y * Sin(d))));
            double M = be.Mu;

            double phi1 = Asin(0.996_647_19 * omega2 * be.Y * Cos(d));
            double phi = ToDegrees(Atan(1.003_364_09 * Tan(phi1)));
            double lambda = To180(M - H - 0.004_178_07 * pbe.DeltaT);
            return new SolarEclipseMapPoint(jd, new CrdsGeographical(lambda, phi));
        }

        /// <summary>
        /// Projects point on Besselian plane to Earth globe.
        /// </summary>
        /// <param name="be">Besselian elements for specified instant.</param>
        /// <param name="p">Point on Besselian plane.</param>
        /// <returns>Geographical coordinates corresponding to the point.</returns>
        private static CrdsGeographical Project(InstantBesselianElements be, PointF p)
        {
            double d = ToRadians(be.D);
            double M = be.Mu;

            double omega = 1.0 / Sqrt(1 - 0.006_694_385 * Cos(d) * Cos(d));

            double x = p.X;
            double y1 = omega * p.Y;
            double b1 = omega * Sin(d);
            double b2 = 0.996_647_19 * omega * Cos(d);
            double B2 = 1 - x * x - y1 * y1;

            if (B2 < 0)
            {
                double P = Atan2(y1, x);
                x = Cos(P);
                y1 = Sin(P);
                B2 = 0;
            }

            double B = Sqrt(B2);
            double H = To360(ToDegrees(Atan2(x, B * b2 - y1 * b1)));
            double phi1 = Asin(B * b1 + y1 * b2);
            double phi = ToDegrees(Atan(1.003_364_09 * Tan(phi1)));
            double lambda = To180(M - H - 0.004_178_07 * be.DeltaT);
            return new CrdsGeographical(lambda, phi);
        }

        /// <summary>
        /// Finds specified curve of solar eclipse map.
        /// </summary>
        /// <param name="pbe">Polynomial Besselian elements.</param>
        /// <param name="i">Type of the curve (see remarks)</param>
        /// <param name="G">Magnitude (see remarks)</param>
        /// <returns>
        /// Point of the eclipse's map curve, for specified longitude, if exists.
        /// </returns>
        /// <remarks>
        /// The method is from book J.Meeus "Elements of Solar Eclipses, 1951-2000, § 22.
        /// <paramref name="i"/> and <paramref name="G"/> define a curve type:
        /// <code>
        /// ------------------------------------------------------------------------------
        /// Type of curve                                          i              G
        /// ------------------------------------------------------------------------------
        /// central line                                           0         irrelevant
        /// northern limit of path of total or annular eclipse    +1             +1
        /// southern limit of path of total or annular eclipse    -1             +1
        /// northern limit of partial eclipse                     +1              0
        /// southern limit of partial eclipse                     -1              0
        /// equal magnitude (north)                               +1         the given G
        /// equal magnitude (south)                               -1         the given G
        /// -------------------------------------------------------------------------------
        /// </code>
        /// </remarks>
        private static IList<CrdsGeographical> FindCurvePoints(PolynomialBesselianElements pbe, int i, int G)
        {
            double[] phis = new double[] { 0, Sign(pbe.Y[0]) * 89.9 };           
            SolarEclipseMapPoint[] prev = new SolarEclipseMapPoint[2];

            var points = new List<SolarEclipseMapPoint>();

            const double step = 1;

            for (double lambda = -180; lambda <= 180; lambda += step)
            {
                for (int k = 0; k < 2; k++)
                {
                    SolarEclipseMapPoint p = FindEclipseCurvePoint(pbe, new CrdsGeographical(lambda, phis[k]), i, G);

                    bool exist = p != null;
                    bool prevExist = prev[k] != null;

                    if (prevExist != exist)
                    {
                        Func<double, bool> func = (lon) => FindEclipseCurvePoint(pbe, new CrdsGeographical(lon, phis[k]), i, G) != null;
                        double lon0 = FindFunctionEnd(func, lambda - step, lambda, prevExist, exist, 0.0001);
                        var p0 = FindEclipseCurvePoint(pbe, new CrdsGeographical(lon0, phis[k]), i, G);

                        if (p0 != null)
                        {
                            if (exist)
                            {
                                double sep = Separation(p0, p);
                                if (sep > 1)
                                {
                                    for (int j = 0; j <= 5; j++)
                                    {
                                        var m = Intermediate(p0, p, (double)j / 5);
                                        var m1 = FindEclipseCurvePoint(pbe, new CrdsGeographical(m.Longitude, phis[k]), i, G);
                                        if (m1 != null)
                                            points.Add(m1);
                                    }
                                }
                            }
                            else
                            {
                                double sep = Separation(prev[k], p0);
                                if (sep > 1)
                                {
                                    for (int j = 0; j <= 5; j++)
                                    {
                                        var m = Intermediate(prev[k], p0, (double)j / 5);
                                        var m1 = FindEclipseCurvePoint(pbe, new CrdsGeographical(m.Longitude, phis[k]), i, G);
                                        if (m1 != null)
                                            points.Add(m1);
                                    }
                                }
                            }

                            points.Add(p0);
                        }
                    }
                    else if (exist)
                    {
                        double sep = Separation(prev[k], p);

                        if (sep > 1)
                        {
                            for (int j = 0; j <= 5; j++)
                            {
                                var m = Intermediate(prev[k], p, (double)j / 5);
                                var m1 = FindEclipseCurvePoint(pbe, new CrdsGeographical(m.Longitude, phis[k]), i, G);
                                if (m1 != null)
                                    points.Add(m1);
                            }
                        }
                    }

                    if (p != null)
                    {
                        points.Add(p);
                    }

                    prev[k] = p;
                }
            }

            if (i == 0)
            {
                points.Add(FindExtremeLimitOfCentralLine(pbe, true));
                points.Add(FindExtremeLimitOfCentralLine(pbe, false));
            }

            // ORDERING POINTS

            if (points.Count <= 2)
            {
                return points.ToArray();
            }
            else
            {
                double jdMin = points.Min(p => p.JulianDay);
                double jdMax = points.Max(p => p.JulianDay);

                double jdMid = (jdMin + jdMax) / 2;

                var mid = points.OrderBy(p => Abs(p.JulianDay - jdMid)).First();

                var jdOrdered = points.OrderBy(p => p.JulianDay).ToList();

                int middleIndex = jdOrdered.IndexOf(mid);

                var left = jdOrdered.Take(middleIndex);
                var right = jdOrdered.Skip(middleIndex + 1);

                var newList = new List<CrdsGeographical>();

                newList.AddRange(left.OrderByDescending(p => Separation(mid, p)));
                newList.Add(mid);
                newList.AddRange(right.OrderBy(p => Separation(mid, p)));

                return newList.ToList();
            }
        }

        /// <summary>
        /// Finds "eclipse on sunrise/sunset" curves for an eclipse.
        /// </summary>
        /// <param name="pbe">Polynomial Besselian elements for the eclipse.</param>
        /// <param name="map">Solar eclipse map.</param>
        /// <param name="jdFrom">Juluan Day value to start from</param>
        /// <param name="jdTo">Juluan Day value to end</param>
        private static void FindRiseSetCurves(PolynomialBesselianElements pbe, SolarEclipseMap map, double jdFrom, double jdTo)
        {
            double deltaJd = jdTo - jdFrom;
            int count = (int)(deltaJd / TimeSpan.FromMinutes(1).TotalDays) + 1;
            double step = deltaJd / count / 20;
            int riseSet = 0;
           
            for (double jd = jdFrom; jd <= jdTo + step * 0.1; jd += step)
            {
                InstantBesselianElements b = pbe.GetInstantBesselianElements(jd);

                // Projection of Moon shadow center on fundamental plane
                PointF pCenter = new PointF((float)b.X, (float)b.Y);

                // Find penumbra (L1 radius) intersection with
                // Earth circle on fundamental plane
                PointF[] pPenumbraIntersect = FindCirclesIntersection(pCenter, b.L1);

                if (Abs(jd - map.P1.JulianDay) < step)
                {
                    CrdsGeographical g = map.P1;
                    if (pCenter.X <= 0)
                        map.RiseSetCurve[riseSet].Insert(0, g);
                    else
                        map.RiseSetCurve[riseSet].Add(g);
                }
                else if (map.P2 != null && Abs(jd - map.P2.JulianDay) < step)
                {
                    CrdsGeographical g = map.P2;
                    if (pCenter.X <= 0)
                        map.RiseSetCurve[riseSet].Insert(0, g);
                    else
                        map.RiseSetCurve[riseSet].Add(g);
                }
                else if (map.P3 != null && Abs(jd - map.P3.JulianDay) < step)
                {
                    CrdsGeographical g = map.P3;
                    if (pCenter.X <= 0)
                        map.RiseSetCurve[riseSet].Insert(0, g);
                    else
                        map.RiseSetCurve[riseSet].Add(g);
                }
                else if (Abs(jd - map.P4.JulianDay) < step)
                {
                    CrdsGeographical g = map.P4;
                    if (pCenter.X <= 0)
                        map.RiseSetCurve[riseSet].Insert(0, g);
                    else
                        map.RiseSetCurve[riseSet].Add(g);
                }
                else
                {
                    for (int i = 0; i < pPenumbraIntersect.Length; i++)
                    {
                        CrdsGeographical g = Project(b, pPenumbraIntersect[i]);
                        
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
        private static PointF[] FindCirclesIntersection(PointF p, double r)
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
        /// Helper method to find end of line based on bisection method.
        /// </summary>
        /// <param name="func">Function describing the line. Should return true of point exists on the line, false otherwise.</param>
        /// <param name="left">Left (lesser) value of argument.</param>
        /// <param name="right">Right (larger) value of argument.</param>
        /// <param name="leftExist">True, if line exist on the left point</param>
        /// <param name="rightExist">True, if line exist on the right point</param>
        /// <param name="epsilon">Desired tolerance.</param>
        /// <returns>Point where the function ends.</returns>
        internal static double FindFunctionEnd(Func<double, bool> func, double left, double right, bool leftExist, bool rightExist, double epsilon)
        {
            double mid = (left + right) / 2;
            bool midExist = func(mid);

            if (Abs(left - right) < epsilon)
            {
                return leftExist ? left : right;
            }

            if (leftExist && !rightExist)
            {
                // 1
                if (midExist)
                {
                    return FindFunctionEnd(func, mid, right, true, false, epsilon);
                }
                // 2
                else
                {
                    return FindFunctionEnd(func, left, mid, true, false, epsilon);
                }
            }
            else if (!leftExist && rightExist)
            {
                // 3
                if (midExist)
                {
                    return FindFunctionEnd(func, left, mid, false, true, epsilon);
                }
                // 4
                else
                {
                    return FindFunctionEnd(func, mid, right, false, true, epsilon);
                }
            }
            else
            {
                throw new Exception();
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
    }
}
