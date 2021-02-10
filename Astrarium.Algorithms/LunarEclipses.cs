using System;
using System.Collections.Generic;
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

        public static int SarosNumber(double jd)
        {
            // Lunar eclipse 21 Jan 2000
            const double jd0 = 2451564.69689;

            // Inex cycle length, in days
            const double I = 10571.95;

            // Saros cycle length, in days
            const double S = 6585.32;

            // Saros number of the lunar eclipse 21 Jan 2000
            const int Saros0 = 124;

            // Dates difference
            double T = jd - jd0;

            int a0 = -1;
            do
            {
                a0++;
                for (int i = 0; i < 2; i++)
                {
                    int a = (i == 0 ? 1 : -1) * a0;
                    int b = (int)Round((T - a * I) / S);
                    double t = a * I + b * S;
                    double dt = Abs(T - t);
                    if (dt <= 2)
                    {
                        return Saros0 + a;
                    }
                }
            }
            while (true);
        }

        /// <summary>
        /// Calculates nearest lunar eclipse (next or previous) for the provided Julian Day.
        /// </summary>
        /// <param name="jd">Julian day of interest, the nearest lunar eclipse for that date will be found.</param>
        /// <param name="next">Flag indicating searching direction. True means searching next eclipse, false means previous.</param>
        public static LunarEclipse NearestEclipse(double jd, bool next)
        {
            //Date d = new Date(jd);
            //double year = d.Year + (Date.JulianEphemerisDay(d) - Date.JulianDay0(d.Year)) / 365.25;
            //double k = Floor((year - 2000) * 12.3685) + 0.5;
            double k = LunarEphem.Lunation(jd, LunationSystem.Meeus) + 0.5;

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

        /// <summary>
        /// Calculates lunar eclipse map
        /// </summary>
        public static LunarEclipseMap EclipseMap(LunarEclipseContacts contacts)
        {
            return new LunarEclipseMap()
            {
                PenumbralBegin = FindCurvePoints(contacts.PenumbralBegin),
                PartialBegin = FindCurvePoints(contacts.PartialBegin),
                TotalBegin = FindCurvePoints(contacts.TotalBegin),
                TotalEnd = FindCurvePoints(contacts.TotalEnd),
                PartialEnd = FindCurvePoints(contacts.PartialEnd),
                PenumbralEnd = FindCurvePoints(contacts.PenumbralEnd)
            };
        }

        private static IList<CrdsGeographical> FindCurvePoints(LunarEclipseContact c)
        {
            if (c == null) 
                return null;

            var curve = new List<CrdsGeographical>();

            var p0 = Project(c, -180);
            curve.Add(new CrdsGeographical(p0.Longitude, -88 * Sign(c.MoonCoordinates.Delta)));

            for (int i = -180; i <= 180; i++)
            {
                curve.Add(Project(c, i));
            }

            var p1 = Project(c, 180);
            curve.Add(new CrdsGeographical(p1.Longitude, -88 * Sign(c.MoonCoordinates.Delta)));

            return curve;
        }

        private static CrdsGeographical Project(LunarEclipseContact c, double L)
        {
            CrdsGeographical g = null;
            CrdsEquatorial eq = new CrdsEquatorial(c.MoonCoordinates);
            double siderealTime = c.SiderealTime;
            double parallax = c.Parallax;
            for (int i = 0; i < 2; i++)
            {
                double hourAngle = Coordinates.HourAngle(siderealTime, 0, eq.Alpha);
                double longitude = To180(L + hourAngle);
                double tanLat = -Cos(ToRadians(L)) / Tan(ToRadians(eq.Delta));
                double latitude = ToDegrees(Atan(tanLat));
                g = new CrdsGeographical(longitude, latitude);
                eq = eq.ToTopocentric(g, siderealTime, parallax);
            }
            return g;
        }

        public static LunarEclipseLocalCircumstances LocalCircumstances(LunarEclipseContacts contacts, CrdsGeographical g)
        {
            return new LunarEclipseLocalCircumstances()
            {
                Location = g,
                PenumbralBegin = GetLocalCircumstancesContactPoint(contacts.PenumbralBegin, g),
                PartialBegin = GetLocalCircumstancesContactPoint(contacts.PartialBegin, g),
                TotalBegin = GetLocalCircumstancesContactPoint(contacts.TotalBegin, g),
                Maximum = GetLocalCircumstancesContactPoint(contacts.Maximum, g),
                TotalEnd = GetLocalCircumstancesContactPoint(contacts.TotalEnd, g),
                PartialEnd = GetLocalCircumstancesContactPoint(contacts.PartialEnd, g),
                PenumbralEnd = GetLocalCircumstancesContactPoint(contacts.PenumbralEnd, g)
            };
        }

        private static LunarEclipseLocalCircumstancesContactPoint GetLocalCircumstancesContactPoint(LunarEclipseContact c, CrdsGeographical g)
        {
            if (c == null)
                return null;

            var h = c.MoonCoordinates.ToTopocentric(g, c.SiderealTime, c.Parallax).ToHorizontal(g, c.SiderealTime);
            return new LunarEclipseLocalCircumstancesContactPoint(c.JuluanDay, h.Altitude);
        }
    }
}
