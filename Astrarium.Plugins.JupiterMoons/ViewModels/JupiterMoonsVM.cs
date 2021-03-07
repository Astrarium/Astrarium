using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.JupiterMoons
{
    public class JupiterMoonsVM : ViewModelBase
    {
        private readonly ISky sky;

        public JupiterMoonsVM(ISky sky)
        {
            this.sky = sky;

            Date now = sky.Context.GetDate(sky.Context.JulianDay);

            double jd0 = new Date(now.Year, now.Month, 1).ToJulianEphemerisDay();
            int daysInMonth = Date.DaysInMonth(now.Year, now.Month);

            var eL = new PointF[5];
            var eB = new PointF[5];
            var eR = new PointF[5];
            var jL = new PointF[5];
            var jB = new PointF[5];
            var jR = new PointF[5];

            // calculate heliocentrical positions of Earth and Jupiter for 5 instants
            // and find least squares approximation model of planets position
            // to quick calculation of Galilean moons postions.
            for (int i = 0; i < 5; i++)
            {
                double jd = jd0 + (i / 4.0) * daysInMonth; 
                var earth = PlanetPositions.GetPlanetCoordinates(3, jd);
                var jupiter = PlanetPositions.GetPlanetCoordinates(5, jd);
                eL[i] = new PointF(i, (float)earth.L);
                eB[i] = new PointF(i, (float)earth.B);
                eR[i] = new PointF(i, (float)earth.R);
                jL[i] = new PointF(i, (float)jupiter.L);
                jB[i] = new PointF(i, (float)jupiter.B);
                jR[i] = new PointF(i, (float)jupiter.R);
            }
           
            // create a model to calculate moons positions
            var model = new PositionsCoefficients()
            {
                Begin = jd0,
                End = jd0 + daysInMonth,
                eL = LeastSquares.FindCoeffs(eL, 3),
                eB = LeastSquares.FindCoeffs(eB, 3),
                eR = LeastSquares.FindCoeffs(eR, 3),
                jL = LeastSquares.FindCoeffs(jL, 3),
                jB = LeastSquares.FindCoeffs(jB, 3),
                jR = LeastSquares.FindCoeffs(jR, 3)
            };

            CrdsRectangular[,] prevPos = null;

            // expect 1 second accuracy
            double eps = TimeSpan.FromSeconds(1).TotalDays;

            // Y-scale stretching, squared (to avoid Jupiter flattening)
            const double STRETCH = 1.14784224788;

            string[] moonNames = { "Io", "Europa", "Ganymede", "Callisto" };

            var events = new List<JovianEvent>();

            // function returns true if moon is occulted by Jupiter
            Func<int, double, bool> isOcculted = (int m, double jd) =>
            {
                var p = model.GetJupiterMoonsPosition(jd)[m, 0];
                return p.Z > 0 && Math.Sqrt(p.X * p.X + p.Y * p.Y * STRETCH) < 1;
            };

            // function returns true if moon is eclipsed by Jupiter
            Func<int, double, bool> isEclipsed = (int m, double jd) =>
            {
                var p = model.GetJupiterMoonsPosition(jd)[m, 1];
                return p.Z > 0 && Math.Sqrt(p.X * p.X + p.Y * p.Y * STRETCH) < 1;
            };


            // for each hour
            for (int h = 0; h <= daysInMonth * 24; h++)
            {
                var pos = model.GetJupiterMoonsPosition(jd0 + h / 24.0);
                
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
                                    model.GetJupiterMoonsPosition(jd)[m, s].X;

                                // instant of max transit/occultation
                                double jd_x0 = FindRoots(f_x0, jd0 + (h - 1) / 24.0, jd0 + h / 24.0, eps);

                                // "Touch" function calculates distance between center of moon
                                // and Jupiter's edge (Y-coordinate is stretched
                                // to compensate Jupiter flattening)
                                Func<double, double> f_touch = (double jd) =>
                                {
                                    var p = model.GetJupiterMoonsPosition(jd)[m, s];
                                    return Math.Sqrt(p.X * p.X + p.Y * p.Y * STRETCH) - 1;
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
                                            Code = $"O{m + 1}",
                                            Text = $"Occultation of {moonNames[m]}",
                                            JdBegin = jdBegin,
                                            JdEnd = jdEnd,
                                            IsEclipsedAtBegin = isEclipsed(m, jdBegin),
                                            IsEclipsedAtEnd = isEclipsed(m, jdEnd)
                                        });
                                    }
                                    // eclipse
                                    else if (occult && s == 1)
                                    {
                                        events.Add(new JovianEvent()
                                        {
                                            Code = $"E{m + 1}",
                                            Text = $"Eclipse of {moonNames[m]}",
                                            JdBegin = jdBegin,
                                            JdEnd = jdEnd,
                                            IsOccultedAtBegin = isOcculted(m, jdBegin),
                                            IsOccultedAtEnd = isOcculted(m, jdEnd)
                                        });
                                    }
                                    // transit of moon
                                    else if (!occult && s == 0)
                                    {
                                        events.Add(new JovianEvent()
                                        {
                                            Code = $"T{m + 1}",
                                            Text = $"Transit of {moonNames[m]}",
                                            JdBegin = jdBegin,
                                            JdEnd = jdEnd,
                                        });
                                    }
                                    // transit of shadow
                                    else if (!occult && s == 1)
                                    {
                                        events.Add(new JovianEvent()
                                        {
                                            Code = $"S{m + 1}",
                                            Text = $"Transit of {moonNames[m]} shadow",
                                            JdBegin = jdBegin,
                                            JdEnd = jdEnd,
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
                                        var p = model.GetJupiterMoonsPosition(jd);
                                        return p[m, s].X - p[n, s].X;
                                    };

                                    // instant of crossing instant
                                    double jd_dx0 = FindRoots(f_dx0, jd0 + (h - 1) / 24.0, jd0 + h / 24.0, eps);
                                    if (!double.IsNaN(jd_dx0))
                                    {
                                        // get positions at the instant
                                        var p = model.GetJupiterMoonsPosition(jd_dx0);

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
                                        double r = model.GetEarthJupiterDistance(jd_dx0);

                                        // distance Sun-Jupiter
                                        double r0 = model.GetSunJupiterDistance(jd_dx0);

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
                                                p = model.GetJupiterMoonsPosition(jd);
                                                dX = p[m, s].X - p[n, s].X;
                                                dY = p[m, s].Y - p[n, s].Y;
                                                return Math.Sqrt(dX * dX + dY * dY) * sd - (sd1 + sd2);
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
                                                        Code = $"{m + 1}O{n + 1}",
                                                        Text = $"{moonNames[m]} occults {moonNames[n]}",
                                                        JdBegin = jdBegin,
                                                        JdEnd = jdEnd,
                                                        IsEclipsedAtBegin = isEclipsed(n, jdBegin),
                                                        IsOccultedAtBegin = isOcculted(n, jdBegin),
                                                        IsEclipsedAtEnd = isEclipsed(n, jdEnd),
                                                        IsOccultedAtEnd = isOcculted(n, jdEnd),
                                                    });
                                                }
                                                else if (s == 1)
                                                {
                                                    events.Add(new JovianEvent()
                                                    {
                                                        Code = $"{m + 1}E{n + 1}",
                                                        Text = $"{moonNames[m]} eclipses {moonNames[n]}",
                                                        JdBegin = jdBegin,
                                                        JdEnd = jdEnd,
                                                        IsEclipsedAtBegin = isEclipsed(n, jdBegin),
                                                        IsOccultedAtBegin = isOcculted(n, jdBegin),
                                                        IsEclipsedAtEnd = isEclipsed(n, jdEnd),
                                                        IsOccultedAtEnd = isOcculted(n, jdEnd),
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

            foreach (var e in events.OrderBy(e => e.JdBegin))
            {
                Debug.WriteLine($"{Formatters.DateTime.Format(new Date(e.JdBegin, sky.Context.GeoLocation.UtcOffset))} - {e.Text} (begin)");
                Debug.WriteLine($"{Formatters.DateTime.Format(new Date(e.JdEnd, sky.Context.GeoLocation.UtcOffset))} - {e.Text} (end)");
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
    }

    public class JovianEvent
    {
        /// <summary>
        /// Julian ephemeris day of beginning of the event.
        /// </summary>
        public double JdBegin { get; set; }

        /// <summary>
        /// Julian ephemeris day of end of the event.
        /// </summary>
        public double JdEnd { get; set; }

        /// <summary>
        /// Event duration, in fractions of day.
        /// </summary>
        public double Duration => JdEnd - JdBegin;

        /// <summary>
        /// Event code.
        /// </summary>
        /// <example>
        /// Event code examples:
        /// JO1 = Jupiter occults Io
        /// JE1 = Jupiter eclipses Io
        /// 2O1 = Europa occults Io
        /// 2E1 = Europa occults Io
        /// </example>
        public string Code { get; set; }

        /// <summary>
        /// Textual description of the event.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Altitude of the Sun at beginning of the event.
        /// </summary>
        public double SunAltBegin { get; set; }

        /// <summary>
        /// Altitude of the Sun at end of the event.
        /// </summary>
        public double SunAltEnd { get; set; }

        /// <summary>
        /// Altitude of the Jupiter at beginning of the event.
        /// </summary>
        public double JupiterAltBegin { get; set; }

        /// <summary>
        /// Altitude of the Jupiter at end of the event.
        /// </summary>
        public double JupiterAltEnd { get; set; }

        /// <summary>
        /// Is the Jovian moon eclipsed by Jupiter at beginning of the event.
        /// </summary>
        public bool IsEclipsedAtBegin { get; set; }

        /// <summary>
        /// Is the Jovian moon eclipsed by Jupiter at end of the event.
        /// </summary>
        public bool IsEclipsedAtEnd { get; set; }

        /// <summary>
        /// Is the Jovian moon occulted by Jupiter at beginning of the event.
        /// </summary>
        public bool IsOccultedAtBegin { get; set; }

        /// <summary>
        /// Is the Jovian moon occulted by Jupiter at end of the event.
        /// </summary>
        public bool IsOccultedAtEnd { get; set; }
    }

    class PositionsCoefficients
    {
        public double Begin { get; set; }
        public double End { get; set; }

        public double[] eL { get; set; }
        public double[] eB { get; set; }
        public double[] eR { get; set; }

        public double[] jL { get; set; }
        public double[] jB { get; set; }
        public double[] jR { get; set; }

        public double GetSunJupiterDistance(double jd)
        {
            double t = (jd - Begin) / (End - Begin) * 4;
            return GetCoeffValue(jR, t);
        }

        public double GetEarthJupiterDistance(double jd)
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

            return jupiter.ToRectangular(earth).ToEcliptical().Distance;
        }

        public CrdsRectangular[,] GetJupiterMoonsPosition(double jd)
        {
            double t = (jd - Begin) / (End - Begin) * 4;
            //if (t < 0 || t > 4)
            //    throw new Exception("Incorrect jd");

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
