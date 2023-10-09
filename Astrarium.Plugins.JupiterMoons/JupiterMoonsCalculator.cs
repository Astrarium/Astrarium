using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.JupiterMoons
{
    public class JupiterMoonsCalculator
    {
        private readonly ISettings settings;

        public double Begin { get; private set; }
        public double End { get; private set; }
        public CrdsGeographical GeoLocation { get; private set; }
        private int daysInMonth;

        private double[] eL;
        private double[] eB;
        private double[] eR;
        private double[] jL;
        private double[] jB;
        private double[] jR;

        public JupiterMoonsCalculator(ISettings settings) 
        {
            this.settings = settings;
        }

        public async Task SetDate(Date date, CrdsGeographical geoLocation)
        {
            await Task.Run(() =>
            {
                double jd0 = date.ToJulianEphemerisDay();
                daysInMonth = Date.DaysInMonth(date.Year, date.Month);

                var eL = new PointF[5];
                var eB = new PointF[5];
                var eR = new PointF[5];
                var jL = new PointF[5];
                var jB = new PointF[5];
                var jR = new PointF[5];

                var earth = new CrdsHeliocentrical[5];
                var jupiter = new CrdsHeliocentrical[5];

                // calculate heliocentrical positions of Earth and Jupiter for 5 instants
                // and find least squares approximation model of planets position
                // to quick calculation of Galilean moons postions.
                for (int i = 0; i < 5; i++)
                {
                    double jd = jd0 + (i / 4.0) * daysInMonth;
                    earth[i] = PlanetPositions.GetPlanetCoordinates(3, jd);
                    jupiter[i] = PlanetPositions.GetPlanetCoordinates(5, jd);
                }

                double[] earthL = earth.Select(x => x.L).ToArray();
                double[] earthB = earth.Select(x => x.B).ToArray();
                double[] earthR = earth.Select(x => x.R).ToArray();

                double[] jupiterL = jupiter.Select(x => x.L).ToArray();
                double[] jupiterB = jupiter.Select(x => x.B).ToArray();
                double[] jupiterR = jupiter.Select(x => x.R).ToArray();

                // it's important to align longitudes (avoid 0 degrees crossing)
                // before applying least squares method
                earthL = Angle.Align(earthL);
                jupiterL = Angle.Align(jupiterL);

                for (int i = 0; i < 5; i++)
                {
                    eL[i] = new PointF(i, (float)earthL[i]);
                    eB[i] = new PointF(i, (float)earthB[i]);
                    eR[i] = new PointF(i, (float)earthR[i]);
                    jL[i] = new PointF(i, (float)jupiterL[i]);
                    jB[i] = new PointF(i, (float)jupiterB[i]);
                    jR[i] = new PointF(i, (float)jupiterR[i]);
                }

                Begin = jd0;
                End = jd0 + daysInMonth;
                GeoLocation = geoLocation;
                this.eL = LeastSquares.FindCoeffs(eL, 3);
                this.eB = LeastSquares.FindCoeffs(eB, 3);
                this.eR = LeastSquares.FindCoeffs(eR, 3);
                this.jL = LeastSquares.FindCoeffs(jL, 3);
                this.jB = LeastSquares.FindCoeffs(jB, 3);
                this.jR = LeastSquares.FindCoeffs(jR, 3);
            });
        }

        public async Task<ICollection<CrdsRectangular[,]>> GetPositions()
        {
            return await Task.Run(() =>
            {
                var positions = new List<CrdsRectangular[,]>();
                for (int h = 0; h <= daysInMonth * 24; h++) 
                {
                    double jd = Begin + h / 24.0;
                    positions.Add(GetJupiterMoonsPosition(jd));
                }
                return positions;
            });
        }

        public async Task<ICollection<JovianEvent>> GetEvents()
        {
            return await Task.Run(() =>
            {
                // previous positions of moons and shadows
                CrdsRectangular[,] prevPos = null;

                // expect 1 second accuracy
                double eps = TimeSpan.FromSeconds(1).TotalDays;

                // Y-scale stretching, squared (to avoid Jupiter flattening)
                const double STRETCH = 1.14784224788;

                // Radii of Galilean moons, in kilometers
                double[] radii = new double[] { 1822, 1561, 2634, 2410 };

                // Equatorial radius of Jupiter, in kilometers
                const double JUP_RADIUS = 69911;

                string[] moonNames = new[] { Text.Get("JupiterMoons.Io"), Text.Get("JupiterMoons.Europa"), Text.Get("JupiterMoons.Ganymede"), Text.Get("JupiterMoons.Callisto") };
                string[] moonNamesGenitive = new[] { Text.Get("JupiterMoons.Io.Genitive"), Text.Get("JupiterMoons.Europa.Genitive"), Text.Get("JupiterMoons.Ganymede.Genitive"), Text.Get("JupiterMoons.Callisto.Genitive") };
                string[] moonNamesDative = new[] { Text.Get("JupiterMoons.Io.Dative"), Text.Get("JupiterMoons.Europa.Dative"), Text.Get("JupiterMoons.Ganymede.Dative"), Text.Get("JupiterMoons.Callisto.Dative") };

                var events = new List<JovianEvent>();

                // function returns true if moon is occulted by Jupiter
                Func<int, double, bool> isOcculted = (int m, double jd) =>
                {
                    var p = GetJupiterMoonsPosition(jd)[m, 0];
                    return p.Z > 0 && Math.Sqrt(p.X * p.X + p.Y * p.Y * STRETCH) < 1;
                };

                // function returns true if moon is eclipsed by Jupiter
                Func<int, double, bool> isEclipsed = (int m, double jd) =>
                {
                    var p = GetJupiterMoonsPosition(jd)[m, 1];
                    return p.Z > 0 && Math.Sqrt(p.X * p.X + p.Y * p.Y * STRETCH) < 1;
                };

                // for each hour
                for (int h = 0; h <= daysInMonth * 24; h++)
                {
                    var pos = GetJupiterMoonsPosition(Begin + h / 24.0);

                    // skip 0 hour (because no previous value yet)
                    if (h > 0)
                    {
                        // s = 0: moon, s = 1: shadow
                        for (int s = 0; s < 2; s++)
                        {
                            // m = moon/shadow index
                            for (int m = 0; m < 4; m++)
                            {
                                // X changes sign => transit/occultation
                                if (prevPos[m, s].X * pos[m, s].X < 0)
                                {
                                    // Z > 0: occulation, Z < 0: transit
                                    bool occult = pos[m, s].Z > 0;

                                    // transit/occultation function
                                    // has zero value when X coordinate is zero
                                    Func<double, double> f_x0 = (double jd) =>
                                        GetJupiterMoonsPosition(jd)[m, s].X;

                                    // instant of max transit/occultation
                                    double jd_x0 = FindRoots(f_x0, Begin + (h - 1) / 24.0, Begin + h / 24.0, eps);

                                    // "Touch" function calculates distance between edge of moon
                                    // and Jupiter's edge (Y-coordinate is stretched
                                    // to compensate Jupiter flattening)
                                    Func<double, double> f_touch = (double jd) =>
                                    {
                                        var p = GetJupiterMoonsPosition(jd)[m, s];
                                        return Math.Sqrt(p.X * p.X + p.Y * p.Y * STRETCH) - 1 - radii[m] / JUP_RADIUS;
                                    };

                                    // event timings
                                    double jdBegin = FindRoots(f_touch, jd_x0 - 3 / 24.0, jd_x0, eps);
                                    double jdEnd = FindRoots(f_touch, jd_x0, jd_x0 + 3 / 24.0, eps);

                                    if (!double.IsNaN(jdBegin) && !double.IsNaN(jdEnd))
                                    {
                                        // occultation
                                        if (occult && s == 0)
                                        {
                                            events.Add(new JovianEvent()
                                            {
                                                MoonNumber = m + 1,
                                                Code = $"O{m + 1}",
                                                Text = Text.Get("JovianEvent.Occultation", ("Name", moonNames[m]), ("GenitiveName", moonNamesGenitive[m])),
                                                JdBegin = jdBegin,
                                                JdEnd = jdEnd,
                                                IsEclipsedAtBegin = isEclipsed(m, jdBegin),
                                                IsEclipsedAtEnd = isEclipsed(m, jdEnd),
                                                JupiterAltBegin = GetJupiterAltitude(jdBegin),
                                                JupiterAltEnd = GetJupiterAltitude(jdEnd),
                                                SunAltBegin = GetSunAltitude(jdBegin),
                                                SunAltEnd = GetSunAltitude(jdEnd)
                                            });
                                        }
                                        // eclipse
                                        else if (occult && s == 1)
                                        {
                                            events.Add(new JovianEvent()
                                            {
                                                MoonNumber = m + 1,
                                                Code = $"E{m + 1}",
                                                Text = Text.Get("JovianEvent.Eclipse", ("Name", moonNames[m]), ("GenitiveName", moonNamesGenitive[m])),
                                                JdBegin = jdBegin,
                                                JdEnd = jdEnd,
                                                IsOccultedAtBegin = isOcculted(m, jdBegin),
                                                IsOccultedAtEnd = isOcculted(m, jdEnd),
                                                JupiterAltBegin = GetJupiterAltitude(jdBegin),
                                                JupiterAltEnd = GetJupiterAltitude(jdEnd),
                                                SunAltBegin = GetSunAltitude(jdBegin),
                                                SunAltEnd = GetSunAltitude(jdEnd)
                                            });
                                        }
                                        // transit of moon
                                        else if (!occult && s == 0)
                                        {
                                            events.Add(new JovianEvent()
                                            {
                                                MoonNumber = m + 1,
                                                Code = $"T{m + 1}",
                                                Text = Text.Get("JovianEvent.Transit", ("Name", moonNames[m]), ("GenitiveName", moonNamesGenitive[m])),
                                                JdBegin = jdBegin,
                                                JdEnd = jdEnd,
                                                JupiterAltBegin = GetJupiterAltitude(jdBegin),
                                                JupiterAltEnd = GetJupiterAltitude(jdEnd),
                                                SunAltBegin = GetSunAltitude(jdBegin),
                                                SunAltEnd = GetSunAltitude(jdEnd)
                                            });
                                        }
                                        // transit of shadow
                                        else if (!occult && s == 1)
                                        {
                                            events.Add(new JovianEvent()
                                            {
                                                Code = $"S{m + 1}",
                                                Text = Text.Get("JovianEvent.ShadowTransit", ("Name", moonNames[m]), ("GenitiveName", moonNamesGenitive[m])),
                                                JdBegin = jdBegin,
                                                JdEnd = jdEnd,
                                                JupiterAltBegin = GetJupiterAltitude(jdBegin),
                                                JupiterAltEnd = GetJupiterAltitude(jdEnd),
                                                SunAltBegin = GetSunAltitude(jdBegin),
                                                SunAltEnd = GetSunAltitude(jdEnd)
                                            });
                                        }
                                    }
                                }

                                // n = another moon index
                                for (int n = 0; n < 4; n++)
                                {
                                    // skip self
                                    if (n == m) continue;

                                    // find difference in X coordinates between
                                    // first moon or shadow and another moon
                                    double dX0 = prevPos[m, s].X - prevPos[n, s].X;
                                    double dX1 = pos[m, s].X - pos[n, s].X;

                                    // dX changes sign => crossing
                                    if (dX0 * dX1 < 0)
                                    {
                                        // crossing function
                                        Func<double, double> f_dx0 = (double jd) =>
                                        {
                                            var pc = GetJupiterMoonsPosition(jd);
                                            return pc[m, s].X - pc[n, s].X;
                                        };

                                        // instant of crossing instant
                                        double jd_dx0 = FindRoots(f_dx0, Begin + (h - 1) / 24.0, Begin + h / 24.0, eps);
                                        if (!double.IsNaN(jd_dx0))
                                        {
                                            // get positions at the instant
                                            var p = GetJupiterMoonsPosition(jd_dx0);

                                            // ignore case when first object (moon/shadow)
                                            // is far than another moon:
                                            // no eclipse/occultation possible 
                                            if (p[m, 0].Z > p[n, 0].Z) continue;

                                            double dX = p[m, s].X - p[n, s].X;
                                            double dY = p[m, s].Y - p[n, s].Y;

                                            // distance between objects,
                                            // in units of Jupiter equatorial radii 
                                            double d = Math.Sqrt(dX * dX + dY * dY);

                                            // distance Earth-Jupiter
                                            double r = GetEarthJupiterDistance(jd_dx0);

                                            // distance Sun-Jupiter
                                            double r0 = GetSunJupiterDistance(jd_dx0);

                                            // Jupiter semidiameter, seconds of arc
                                            double sd = PlanetEphem.Semidiameter(5, r);

                                            // first object (moon/shadow) semidiameter:
                                            double sd1 = s == 0 ?
                                                GalileanMoons.MoonSemidiameter(r, p[m, 0].Z, m) :
                                                GalileanMoons.Shadow(r, r0, m, p[m, 1], p[n, 1]).Umbra;

                                            // another moon semidiameter
                                            double sd2 = GalileanMoons.MoonSemidiameter(r, p[n, 0].Z, n);

                                            // if distance between objects is less
                                            // than sum of semidiameters, then event takes place
                                            if (d * sd < sd1 + sd2)
                                            {
                                                // "Touch" function: has zero value wnen
                                                // two objects (moon/shadow and another moon)
                                                // touches with their edges
                                                Func<double, double> f_touch = (double jd) =>
                                                {
                                                    var pt = GetJupiterMoonsPosition(jd);
                                                    var dX_ = pt[m, s].X - pt[n, s].X;
                                                    var dY_ = pt[m, s].Y - pt[n, s].Y;
                                                    return Math.Sqrt(dX_ * dX_ + dY_ * dY_) - (sd1 + sd2) / sd;
                                                };

                                                // begin and end of event
                                                double jdBegin = FindRoots(f_touch, jd_dx0 - 1 / 24.0, jd_dx0, eps);
                                                double jdEnd = FindRoots(f_touch, jd_dx0, jd_dx0 + 1 / 24.0, eps);

                                                if (!double.IsNaN(jdBegin) && !double.IsNaN(jdEnd))
                                                {
                                                    if (s == 0)
                                                    {
                                                        events.Add(new JovianEvent()
                                                        {
                                                            MoonNumber = n + 1,
                                                            Code = $"{m + 1}O{n + 1}",
                                                            Text = Text.Get("JovianEvent.MutualOccultation", 
                                                            ("Name", moonNames[m]), ("OccultedName", moonNames[n]),
                                                            ("GenitiveName", moonNamesGenitive[m]), ("OccultedGenitiveName", moonNamesGenitive[n]),
                                                            ("DativeName", moonNamesDative[m]), ("OccultedDativeName", moonNamesDative[n])),
                                                            JdBegin = jdBegin,
                                                            JdEnd = jdEnd,
                                                            IsEclipsedAtBegin = isEclipsed(n, jdBegin),
                                                            IsOccultedAtBegin = isOcculted(n, jdBegin),
                                                            IsEclipsedAtEnd = isEclipsed(n, jdEnd),
                                                            IsOccultedAtEnd = isOcculted(n, jdEnd),
                                                            JupiterAltBegin = GetJupiterAltitude(jdBegin),
                                                            JupiterAltEnd = GetJupiterAltitude(jdEnd),
                                                            SunAltBegin = GetSunAltitude(jdBegin),
                                                            SunAltEnd = GetSunAltitude(jdEnd),
                                                        });
                                                    }
                                                    else if (s == 1)
                                                    {
                                                        events.Add(new JovianEvent()
                                                        {
                                                            MoonNumber = n + 1,
                                                            Code = $"{m + 1}E{n + 1}",
                                                            Text = Text.Get("JovianEvent.MutualEclipse",
                                                            ("Name", moonNames[m]), ("EclipsedName", moonNames[n]),
                                                            ("GenitiveName", moonNamesGenitive[m]), ("EclipsedGenitiveName", moonNamesGenitive[n]),
                                                            ("DativeName", moonNamesDative[m]), ("EclipsedDativeName", moonNamesDative[n])),
                                                            JdBegin = jdBegin,
                                                            JdEnd = jdEnd,
                                                            IsEclipsedAtBegin = isEclipsed(n, jdBegin),
                                                            IsOccultedAtBegin = isOcculted(n, jdBegin),
                                                            IsEclipsedAtEnd = isEclipsed(n, jdEnd),
                                                            IsOccultedAtEnd = isOcculted(n, jdEnd),
                                                            JupiterAltBegin = GetJupiterAltitude(jdBegin),
                                                            JupiterAltEnd = GetJupiterAltitude(jdEnd),
                                                            SunAltBegin = GetSunAltitude(jdBegin),
                                                            SunAltEnd = GetSunAltitude(jdEnd),
                                                        });
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    prevPos = pos;
                }

                return events.Where(e =>
                    e.JdBegin >= Begin &&
                    e.JdEnd <= End + daysInMonth &&
                    !(e.IsEclipsedAtBegin && e.IsEclipsedAtEnd) &&
                    !(e.IsOccultedAtBegin && e.IsOccultedAtEnd))
                .ToArray();
            });
        }

        public async Task<ICollection<GRSEvent>> GetGRSTimes()
        {
            return await Task.Run(() =>
            {
                var events = new List<GRSEvent>();

                var grsLongitudeSettings = settings.Get<GreatRedSpotSettings>("GRSLongitude");

                // Jupiter rotation speed (system II), degress per day
                const double ROT_SPEED = 870.1869147;

                // Two hours, expressed in fractions of day
                const double TWO_HOURS = 1.0 / 12.0;

                double jd = Begin - 1;
                do
                {
                    jd += 360.0 / ROT_SPEED;

                    // GRS longitude
                    double grsLongitude = PlanetEphem.GreatRedSpotLongitude(jd, grsLongitudeSettings);

                    // Central Meridian longitude (system II)
                    double CM = PlanetEphem.JupiterCM2(jd);

                    // Difference between CM longitude and GRS longitude
                    double delta = Angle.To360(CM - grsLongitude);

                    if (delta > 180)
                    {
                        delta -= 360;
                    }

                    // Correction
                    jd = jd - delta / ROT_SPEED;

                    if (jd >= Begin && jd <= End)
                    {
                        var ev = new GRSEvent()
                        {
                            JdTransit = jd,
                            JdAppear = jd - TWO_HOURS,
                            JdDisappear = jd + TWO_HOURS,
                            SunAltTransit = GetSunAltitude(jd),
                            JupiterAltTransit = GetJupiterAltitude(jd),
                            SunAltAppear = GetSunAltitude(jd - TWO_HOURS),
                            JupiterAltAppear = GetJupiterAltitude(jd - TWO_HOURS),
                            SunAltDisappear = GetSunAltitude(jd + TWO_HOURS),
                            JupiterAltDisappear = GetJupiterAltitude(jd + TWO_HOURS)                            
                        };
                        events.Add(ev);
                    }
                }
                while (jd <= End);

                return events;
            });
        }

        /// <summary>
        /// Finds function root by bisection method
        /// </summary>
        /// <param name="func">Function to find root</param>
        /// <param name="a">Left edge of the interval</param>
        /// <param name="b">Right edge of the interval</param>
        /// <param name="eps">Tolerance</param>
        /// <returns>Function root</returns>
        private double FindRoots(Func<double, double> func, double a, double b, double eps)
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

        private double GetJupiterAltitude(double jd)
        {
            // Nutation elements
            var nutation = Nutation.NutationElements(jd);

            // True obliquity
            var epsilon = Date.TrueObliquity(jd, nutation.deltaEpsilon);

            // Greenwich apparent sidereal time 
            double siderealTime = Date.ApparentSiderealTime(jd, nutation.deltaPsi, epsilon);

            // Ecliptical coordinates of Jupiter
            var ecl = GetJupiterEcliptical(jd);

            // Equatorial geocentric coordinates of Jupiter
            var eq0 = ecl.ToEquatorial(epsilon);

            // Equatorial topocentric coordinates of Jupiter
            var eq = eq0.ToTopocentric(GeoLocation, siderealTime, PlanetEphem.Parallax(ecl.Distance));

            // Horizontal coordinates of Jupiter
            return eq.ToHorizontal(GeoLocation, siderealTime).Altitude;
        }

        private CrdsEcliptical GetJupiterEcliptical(double jd)
        {
            double t = (jd - Begin) / (End - Begin) * 4;

            var earth = new CrdsHeliocentrical()
            {
                L = GetCoeffValue(eL, t),
                B = GetCoeffValue(eB, t),
                R = GetCoeffValue(eR, t),
            };

            var jupiter = new CrdsHeliocentrical()
            {
                L = GetCoeffValue(jL, t),
                B = GetCoeffValue(jB, t),
                R = GetCoeffValue(jR, t),
            };

            return jupiter.ToRectangular(earth).ToEcliptical();
        }

        private CrdsEcliptical GetSunEcliptical(double jd)
        {
            double t = (jd - Begin) / (End - Begin) * 4;

            var earth = new CrdsHeliocentrical()
            {
                L = GetCoeffValue(eL, t),
                B = GetCoeffValue(eB, t),
                R = GetCoeffValue(eR, t),
            };

            // Ecliptical coordinates of Sun
            return new CrdsEcliptical(Angle.To360(earth.L + 180), -earth.B, earth.R);            
        }

        private double GetSunAltitude(double jd)
        {
            // Nutation elements
            var nutation = Nutation.NutationElements(jd);

            // True obliquity
            var epsilon = Date.TrueObliquity(jd, nutation.deltaEpsilon);

            // Greenwich apparent sidereal time 
            double siderealTime = Date.ApparentSiderealTime(jd, nutation.deltaPsi, epsilon);

            var ecl = GetSunEcliptical(jd);

            // Equatorial geocentric coordinates of Sun
            var eq0 = ecl.ToEquatorial(epsilon);

            // Equatorial topocentric coordinates of Sun
            var eq = eq0.ToTopocentric(GeoLocation, siderealTime, PlanetEphem.Parallax(ecl.Distance));

            // Altitude of Sun
            return eq.ToHorizontal(GeoLocation, siderealTime).Altitude;
        }

        private double GetSunJupiterDistance(double jd)
        {
            double t = (jd - Begin) / (End - Begin) * 4;
            return GetCoeffValue(jR, t);
        }

        private double GetEarthJupiterDistance(double jd)
        {
            return GetJupiterEcliptical(jd).Distance;
        }

        private CrdsRectangular[,] GetJupiterMoonsPosition(double jd)
        {
            double t = (jd - Begin) / (End - Begin) * 4;

            var earth = new CrdsHeliocentrical()
            {
                L = GetCoeffValue(eL, t),
                B = GetCoeffValue(eB, t),
                R = GetCoeffValue(eR, t),
            };

            var jupiter = new CrdsHeliocentrical()
            {
                L = GetCoeffValue(jL, t),
                B = GetCoeffValue(jB, t),
                R = GetCoeffValue(jR, t),
            };

            return GalileanMoons.Positions(jd, earth, jupiter);
        }

        private double GetCoeffValue(double[] coeff, double t) => coeff.Select((y, n) => y * Math.Pow(t, n)).Sum();
    }
}
