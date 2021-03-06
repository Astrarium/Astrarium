﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using static Astrarium.Algorithms.Angle;

namespace Astrarium.Algorithms
{
    public static class NeptunianMoons
    {
        /// <summary>
        /// Gets ecliptical coordinates of Neptunian moon
        /// </summary>
        /// <param name="jd">Julian day of calculation</param>
        /// <param name="neptune">Ecliptical coordinates of Neptune for the specified date</param>
        /// <param name="index">Moon index, 1 = Triton, 2 = Nereid</param>
        /// <returns></returns>
        public static CrdsEcliptical Position(double jd, CrdsEcliptical neptune, int index)
        {
            if (index == 1)
                return TritonPosition(jd, neptune);
            else if (index == 2)
                return NereidPosition(jd, neptune);
            else
                throw new ArgumentException("Incorrect moon index", nameof(index));
        }

        /// <summary>
        /// Moon mean radius, in km
        /// </summary>
        private static double[] MOON_RADIUS = new double[] { 1354.0, 170.0 };

        /// <summary>
        /// Gets visible semidiameter of Neptunian moon, in seconds of arc 
        /// </summary>
        /// <param name="distance">Distance from Earth, in a.u.</param>
        /// <param name="index">Moon index, 1 = Triton, 2 = Nereid</param>
        /// <returns>
        /// Visible semidiameter of the moon, in seconds of arc
        /// </returns>
        public static double Semidiameter(double distance, int index)
        {
            if (index == 1 || index == 2)
                return ToDegrees(Atan(MOON_RADIUS[index - 1] / (distance * 149597870.0))) * 3600;
            else
                throw new ArgumentException("Incorrect moon index", nameof(index));
        }

        /// <summary>
        /// Absolute magnitudes
        /// </summary>
        private static double[] MOON_MAGNIUDE = new double[] { -1.24, 4.0 };

        public static float Magnitude(double Delta, double r, int index)
        {
            if (index == 1 || index == 2)
                return (float)(MOON_MAGNIUDE[index - 1] + 5 * Math.Log10(r * Delta));
            else
                throw new ArgumentException("Incorrect moon index", nameof(index));
        }

        /// <summary>
        /// Calculates ecliptical coordinates of Triton, largest moon of Neptune.
        /// </summary>
        /// <param name="jd">Julian Day of calculation</param>
        /// <param name="neptune">Ecliptical coordinates of Neptune for the Julian Day specified.</param>
        /// <returns>Ecliptical coordinates of Triton for specified date.</returns>
        /// <remarks>
        /// 
        /// The method is based on following works:
        /// 
        /// 1. Harris, A.W. (1984), "Physical Properties of Neptune and Triton Inferred from the Orbit of Triton" NASA CP-2330, pages 357-373:
        ///    http://articles.adsabs.harvard.edu/cgi-bin/nph-iarticle_query?1984NASCP2330..357H&defaultprint=YES&filetype=.pdf
        /// 
        /// 2. Seidelmann, P. K.: Explanatory Supplement to The Astronomical Almanac, 
        ///    University Science Book, Mill Valley (California), 1992,
        ///    Chapter 6 "Orbital Ephemerides and Rings of Satellites", page 373, 6.61-1 Triton
        ///    https://archive.org/download/131123ExplanatorySupplementAstronomicalAlmanac/131123-explanatory-supplement-astronomical-almanac.pdf
        ///    
        /// </remarks>
        private static CrdsEcliptical TritonPosition(double jd, CrdsEcliptical neptune)
        {
            NutationElements ne = Nutation.NutationElements(jd);
            double epsilon = Date.TrueObliquity(jd, ne.deltaEpsilon);

            // convert current coordinates to J1950 epoch, as algorithm requires
            CrdsEquatorial eq = neptune.ToEquatorial(epsilon);
            PrecessionalElements pe1950 = Precession.ElementsFK5(jd, Date.EPOCH_J1950);
            CrdsEquatorial eqNeptune1950 = Precession.GetEquatorialCoordinates(eq, pe1950);

            const double t0 = 2433282.5;     // 1.0 Jan 1950                
            const double a = 0.0023683;      // semimajor axis of Triton, in a.u.

            const double n = 61.2588532;     // nodal mean motion, degrees per day
            const double lambda0 = 200.913;  // longitude from ascending node through the invariable plane at epoch
            const double i = 158.996;        // inclination of orbit to the invariable plane

            const double Omega0 = 151.401;   // angle from the intersection of invariable plane with the earth's 
                                             // equatorial plane of 1950.0 to the ascending node 
                                             // of the orbit through the invariable plane

            const double OmegaDot = 0.57806; // nodal precision rate, degrees per year

            // Calculate J2000.0 RA and Declination of the pole of the invariable plane
            // These formulae are taken from the book: 
            // Seidelmann, P. K.: Explanatory Supplement to The Astronomical Almanac, 
            // University Science Book, Mill Valley (California), 1992,
            // Chapter 6 "Orbital Ephemerides and Rings of Satellites", page 373, 6.61-1 Triton
            double T = (jd - 2451545.0) / 36525.0;
            double N = ToRadians(359.28 + 54.308 * T);
            double ap = 298.72 + 2.58 * Sin(N) - 0.04 * Sin(2 * N);
            double dp = 42.63 - 1.90 * Cos(N) + 0.01 * Cos(2 * N);

            // Convert pole coordinates to J1950
            CrdsEquatorial eqPole1950 = Precession.GetEquatorialCoordinates(new CrdsEquatorial(ap, dp), pe1950);
            ap = eqPole1950.Alpha;
            dp = eqPole1950.Delta;

            // take light-time effect into account
            double tau = PlanetPositions.LightTimeEffect(neptune.Distance);

            double lambda = To360(lambda0 + n * (jd - t0 - tau));
            double omega = Omega0 + OmegaDot * (jd - t0 - tau) / 365.25;

            // cartesian state vector of Triton
            var r =
                Matrix.R3(ToRadians(-ap - 90)) *
                Matrix.R1(ToRadians(dp - 90)) *
                Matrix.R3(ToRadians(-omega)) *
                Matrix.R1(ToRadians(-i)) *
                new Matrix(new[,] { { a * Cos(ToRadians(lambda)) }, { a * Sin(ToRadians(lambda)) }, { 0 } });

            // normalize by distance to Neptune
            r.Values[0, 0] /= neptune.Distance;
            r.Values[1, 0] /= neptune.Distance;
            r.Values[2, 0] /= neptune.Distance;

            // offsets vector
            var d =
                Matrix.R2(ToRadians(-eqNeptune1950.Delta)) *
                Matrix.R3(ToRadians(eqNeptune1950.Alpha)) *
                r;

            // radial component, positive away from observer
            // converted to degrees
            double x = ToDegrees(d.Values[0, 0]);

            // semimajor axis, expressed in degrees, as visible from Earth
            double theta = ToDegrees(Atan(a / neptune.Distance));

            // offsets values in degrees           
            double dAlphaCosDelta = ToDegrees(d.Values[1, 0]);
            double dDelta = ToDegrees(d.Values[2, 0]);

            double delta = eqNeptune1950.Delta + dDelta;
            double dAlpha = dAlphaCosDelta / Cos(ToRadians(eqNeptune1950.Delta));
            double alpha = eqNeptune1950.Alpha + dAlpha;

            CrdsEquatorial eqTriton1950 = new CrdsEquatorial(alpha, delta);

            // convert J1950 equatorial coordinates to current epoch
            // and to ecliptical
            PrecessionalElements pe = Precession.ElementsFK5(Date.EPOCH_J1950, jd);
            CrdsEquatorial eqTriton = Precession.GetEquatorialCoordinates(eqTriton1950, pe);
            CrdsEcliptical eclTriton = eqTriton.ToEcliptical(epsilon);

            // calculate distance to Earth
            eclTriton.Distance = neptune.Distance + x / theta * a;

            return eclTriton;
        }

        /// <summary>
        /// Calculates ecliptical coordinates of Nereid, the third-largest moon of Neptune.
        /// </summary>
        /// <param name="jd">Julian Day of calculation</param>
        /// <param name="neptune">Ecliptical coordinates of Neptune for the Julian Day specified.</param>
        /// <returns>Ecliptical coordinates of Nereid for specified date.</returns>
        /// <remarks>
        /// 
        /// The method is based on work of F. Mignard (1981), "The Mean Elements of Nereid", 
        /// The Astronomical Journal, Vol 86, Number 11, pages 1728-1729
        /// The work can be found by link: http://adsabs.harvard.edu/full/1981AJ.....86.1728M
        /// 
        /// There are some changes from the original algorithm were made,
        /// to be compliant with ephemeris provided by Nasa JPL Horizons system (https://ssd.jpl.nasa.gov/?ephemerides):
        /// 
        /// 1. Other value of mean motion (n) is used: 
        ///    - original work : n = 0.999552
        ///    - implementation: n = 360.0 / 360.1362 (where 360.1362 is an orbital period)
        /// 
        /// 2. Rotation around Z axis by angle OmegaN should by taken with NEGATIVE sign,
        ///    insted of POSITIVE sign in original work (possible typo?),
        ///    note the NEGATIVE sign for "Ne" angle (same meaning as "OmegaN" in original work) in the book:
        ///    Seidelmann, P. K.: Explanatory Supplement to The Astronomical Almanac, 
        ///    University Science Book, Mill Valley (California), 1992,
        ///    Chapter 6 "Orbital Ephemerides and Rings of Satellites", page 376, formula 6.62-3
        ///    
        /// </remarks>
        private static CrdsEcliptical NereidPosition(double jd, CrdsEcliptical neptune)
        {
            NutationElements ne = Nutation.NutationElements(jd);
            double epsilon = Date.TrueObliquity(jd, ne.deltaEpsilon);

            // convert current coordinates to J1950 epoch, as algorithm requires
            CrdsEquatorial eq = neptune.ToEquatorial(epsilon);
            PrecessionalElements pe1950 = Precession.ElementsFK5(jd, Date.EPOCH_J1950);
            CrdsEquatorial eqNeptune1950 = Precession.GetEquatorialCoordinates(eq, pe1950);

            const double jd0 = 2433680.5;       // Initial Epoch: 3.0 Feb 1951

            const double a = 0.036868;          // Semi-major axis, in a.u.
            const double e0 = 0.74515;          // Orbit eccentricity for jd0 epoch
            const double i0 = 10.041;           // Inclination of the orbit for jd0 epoch, in degrees
            const double Omega0 = 329.3;        // Longitude of the node of the orbit for jd0 epoch, in degrees
            const double M0 = 358.91;           // Mean anomaly for jd0 epoch, in degrees
            const double n = 360.0 / 360.1362;  // Mean motion, in degrees per day
            const double OmegaN = 3.552;        // Longitude of ascending node of the orbit of Neptune, for J1950.0 epoch, in degrees
            const double gamma = 22.313;        // Inclination of the orbit of Neptune, for J1950.0 epoch, in degrees

            // take light-time effect into account
            double tau = PlanetPositions.LightTimeEffect(neptune.Distance);

            double t = jd - tau - jd0;          // in days
            double T = t / 36525.0;             // in Julian centuries

            double psi = ToRadians(To360(282.9 + 2.68 * T));
            double twoTheta = ToRadians(To360(107.4 + 0.01196 * t));

            // Equation to found omega, argument of pericenter
            Func<double, double> omegaEquation = (om) => To360(282.9 + 2.68 * T - 19.25 * Sin(2 * psi) + 3.23 * Sin(4 * psi) - 0.725 * Sin(6 * psi) - 0.351 * Sin(twoTheta) - 0.7 * Sin(ToRadians(2 * om) - twoTheta)) - om;

            // Solve equation (find root: omega value)
            double omega = ToRadians(FindRoots(omegaEquation, 0, 360, 1e-8));

            // Find longitude of the node
            double Omega = Omega0 - 2.4 * T + 19.7 * Sin(2 * psi) - 3.3 * Sin(4 * psi) + 0.7 * Sin(6 * psi) + 0.357 * Sin(twoTheta) + 0.276 * Sin(2 * omega - twoTheta);

            // Find orbit eccentricity
            double e = e0 - 0.006 * Cos(2 * psi) + 0.0056 * Cos(2 * omega - twoTheta);

            // Find mean anomaly
            double M = To360(M0 + n * t - 0.38 * Sin(2 * psi) + 1.0 * Sin(2 * omega - twoTheta));

            // Find inclination
            double cosi = Cos(ToRadians(i0)) - 9.4e-3 * Cos(2 * psi);
            double i = Acos(cosi);

            // Find eccentric anomaly by solving Kepler equation
            double E = SolveKepler(M, e);

            double X = a * (Cos(E) - e);
            double Y = a * Sqrt(1 - e * e) * Sin(E);

            Matrix d =
                Matrix.R2(ToRadians(-eqNeptune1950.Delta)) *
                Matrix.R3(ToRadians(eqNeptune1950.Alpha)) *
                Matrix.R3(ToRadians(-OmegaN)) *
                Matrix.R1(ToRadians(-gamma)) *
                Matrix.R3(ToRadians(-Omega)) *
                Matrix.R1(-i) *
                Matrix.R3(-omega) *
                new Matrix(new double[,] { { X / neptune.Distance }, { Y / neptune.Distance }, { 0 } });

            // radial component, positive away from observer
            // converted to degrees
            double x = ToDegrees(d.Values[0, 0]);

            // offsets values in degrees           
            double dAlphaCosDelta = ToDegrees(d.Values[1, 0]);
            double dDelta = ToDegrees(d.Values[2, 0]);

            double delta = eqNeptune1950.Delta + dDelta;
            double dAlpha = dAlphaCosDelta / Cos(ToRadians(eqNeptune1950.Delta));
            double alpha = eqNeptune1950.Alpha + dAlpha;

            CrdsEquatorial eqNereid1950 = new CrdsEquatorial(alpha, delta);

            // convert J1950 equatorial coordinates to current epoch
            // and to ecliptical
            PrecessionalElements pe = Precession.ElementsFK5(Date.EPOCH_J1950, jd);
            CrdsEquatorial eqNereid = Precession.GetEquatorialCoordinates(eqNereid1950, pe);
            CrdsEcliptical eclNereid = eqNereid.ToEcliptical(epsilon);

            // semimajor axis, expressed in degrees, as visible from Earth
            double theta = ToDegrees(Atan(a / neptune.Distance));

            // calculate distance to Earth
            eclNereid.Distance = neptune.Distance + x / theta * a;

            return eclNereid;
        }

        /// <summary>
        /// Solves Kepler equation
        /// </summary>
        /// <param name="M">Mean anomaly, in degrees</param>
        /// <param name="e">Eccentricity</param>
        /// <returns>Eccentric anomaly, in radians</returns>
        private static double SolveKepler(double M, double e)
        {
            M = ToRadians(M);
            double E0;
            double E1 = M;
            double M_ = M;
            do
            {
                E0 = E1;
                E1 = M_ + e * Sin(E0);
            } while (Abs(E1 - E0) >= 1e-9);
            return E1;
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
