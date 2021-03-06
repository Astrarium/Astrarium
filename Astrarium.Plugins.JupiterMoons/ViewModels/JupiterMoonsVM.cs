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

                                // function calculates distance between center of moon
                                // and Jupiter's edge (Y-coordinate is stretched
                                // to compensate Jupiter flattening)
                                Func<double, double> func = (double jd) =>
                                {
                                    var p = model.GetJupiterMoonsPosition(jd)[m, s];
                                    return Math.Sqrt(p.X * p.X + p.Y * p.Y * STRETCH) - 1;
                                };

                                // function returns true if moon is occulted
                                Func<double, bool> isOcculted = (double jd) =>
                                {
                                    var p = model.GetJupiterMoonsPosition(jd)[m, 0];
                                    return p.Z > 0 && Math.Sqrt(p.X * p.X + p.Y * p.Y * STRETCH) < 1;
                                };

                                // begin of event
                                double jdBegin = FindRoots(func, jd_x0 - 3 / 24.0, jd_x0, eps);
                                if (!double.IsNaN(jdBegin))
                                {
                                    // occultation
                                    if (occult && s == 0)
                                        Debug.WriteLine($"{jdBegin.ToString(CultureInfo.InvariantCulture)}: Jupiter begins to occult {moonNames[m]}");
                                    // eclipse, need to check planet is not occulted at that time 
                                    else if (occult && s == 1 && !isOcculted(jdBegin))
                                        Debug.WriteLine($"{jdBegin.ToString(CultureInfo.InvariantCulture)}: Jupiter begins to eclipse {moonNames[m]}");
                                    // transit of moon
                                    else if (!occult && s == 0)
                                        Debug.WriteLine($"{jdBegin.ToString(CultureInfo.InvariantCulture)}: {moonNames[m]} begins transit of Jupiter");
                                    // transit of shadow
                                    else if (!occult && s == 1)
                                        Debug.WriteLine($"{jdBegin.ToString(CultureInfo.InvariantCulture)}: {moonNames[m]}'s shadow enters Jupiter");
                                }

                                // end of event
                                double jdEnd = FindRoots(func, jd_x0, jd_x0 + 3 / 24.0, eps);
                                if (!double.IsNaN(jdEnd))
                                {
                                    // occultation
                                    if (occult && s == 0)
                                        Debug.WriteLine($"{jdEnd.ToString(CultureInfo.InvariantCulture)}: Jupiter ends to occult {moonNames[m]}");
                                    // eclipse, need to check planet is not occulted at that time 
                                    else if (occult && s == 1 && !isOcculted(jdEnd))
                                        Debug.WriteLine($"{jdEnd.ToString(CultureInfo.InvariantCulture)}: Jupiter ends to eclipse {moonNames[m]}");
                                    // transit of moon
                                    else if (!occult && s == 0)
                                        Debug.WriteLine($"{jdEnd.ToString(CultureInfo.InvariantCulture)}: {moonNames[m]} ends transit of Jupiter");
                                    // transit of shadow
                                    else if (!occult && s == 1)
                                        Debug.WriteLine($"{jdEnd.ToString(CultureInfo.InvariantCulture)}: {moonNames[m]}'s shadow leaves Jupiter");
                                }
                            }

                            // n = another moon index
                            // for that moon we don't condider shadow position,
                            // so second index of always zero (=moon).
                            for (int n = 0; n < 4; n++)
                            {
                                // skip self
                                if (n == m) continue;

                                // find difference in X coordinates between
                                // first moon or shadow and another moon
                                double dX0 = prevPos[m, s].X - prevPos[n, 0].X;
                                double dX1 = pos[m, s].X - pos[n, 0].X;

                                // dX changes sign => crossing
                                if (dX0 * dX1 < 0)
                                {
                                    // crossing function
                                    // has zero value when dX is zero
                                    Func<double, double> f_dx0 = (double jd) =>
                                    {
                                        var p = model.GetJupiterMoonsPosition(jd);
                                        return p[m, s].X - p[n, 0].X;
                                    };

                                    // instant of crossing instant
                                    double jd_dx0 = FindRoots(f_dx0, jd0 + (h - 1) / 24.0, jd0 + h / 24.0, eps);
                                    if (!double.IsNaN(jd_dx0))
                                    {
                                        // get positions at the instant
                                        var p = model.GetJupiterMoonsPosition(jd_dx0);

                                        // ignore case when first object (moon/shadow)
                                        // is far than second moon:
                                        // no eclipse/occultation possible 
                                        if (p[m, s].Z > p[n, 0].Z) continue;

                                        double dX = p[m, s].X - p[n, 0].X;
                                        double dY = p[m, s].Y - p[n, 0].Y;

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
                                        double r1 = s == 0 ?
                                            GalileanMoons.MoonSemidiameter(r, p[m, 0].Z, m) :

                                            // TODO: this is incorrect, check!
                                            GalileanMoons.Shadow(r, r0, m, p[m, 1], p[n, 0]).Penumbra;
                                                                                
                                        double r2 = GalileanMoons.MoonSemidiameter(r, p[n, 0].Z, n);

                                        if (d * sd < r1 + r2)
                                        {
                                            if (s == 0)
                                                Debug.WriteLine($"{jd_dx0.ToString(CultureInfo.InvariantCulture)}: {moonNames[m]} occults {moonNames[n]}");
                                            else if (s == 1)
                                                Debug.WriteLine($"{jd_dx0.ToString(CultureInfo.InvariantCulture)}: {moonNames[m]} eclipses {moonNames[n]}");
                                        } 
                                    }
                                }
                            }
                        }
                    }
                }
                prevPos = pos;   
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
