using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using static ADK.Angle;

namespace ADK
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
            double epsilon = Date.MeanObliquity(jd);
            CrdsEquatorial eq = new CrdsEcliptical(0, 90).ToEquatorial(epsilon);

            if (index == 1)
                return TritonPosition(jd, neptune);
            else if (index == 2)
                return NereidPosition(jd, neptune);
            else if (index == 3)

            // Sample request for NASA JPL Horizons:
            // https://ssd.jpl.nasa.gov/horizons_batch.cgi?batch=1&COMMAND='802'&CENTER='500@899'&MAKE_EPHEM='YES'&TABLE_TYPE='ELEMENTS'&START_TIME='2020-02-01'&STOP_TIME='2020-02-02'&STEP_SIZE='2 d'&OUT_UNITS='AU-D'&REF_PLANE='ECLIPTIC'&REF_SYSTEM='J2000'&TP_TYPE='ABSOLUTE'&CSV_FORMAT='YES'&OBJ_DATA='YES'

            // Nereid, ecliptical plane
            return GenericPosition(jd, new Orbit()
                {
                    jd0 = 2458880.500000000, // = A.D. 2020 - Feb - 01 00:00:00.0000 TDB
                    M0 = 3.494900228078851E+02, // Mean anomaly (mean value) 
                    n = 9.959605978733523E-01, // Longitude rate(mean value)
                    e = 7.456407980998859E-01, // 	Eccentricity (mean value)
                    a = 3.694755294409791E-02,
                    i = 5.065310902937719E+00,
                    omega0 = 2.966067788105880E+02,
                    node0 = 3.196007803495440E+02, 


                    Pw = 8091.45 * 365.25,
                    Pnode = 9455.73 * 365.25,
                }, neptune);

            else if (index == 4)
            {
                //// Triton. this works
                ///*
                //Ephemeris Type [change] : 	ELEMENTS
                //Target Body [change] : 	Triton (NI) [801]
                //Center [change] : 	Neptune (body center) [500@899]
                //Time Span [change] : 	Start=2020-02-26, Stop=2020-02-27, Step=1 d
                //Table Settings [change] : 	reference plane=BODY EQUATOR
                //                            reference plane :body mean equator and node of date
                //                            reference system: ICRF/J2000.0
                //                            TP type: absolute
                //Display/Output [change] : 	plain text
                // */

                //return GenericPosition(jd, new Orbit()
                //{
                //    epoch = Date.EPOCH_J2000, // Reference plane epoch
                //    jd0 = 2458905.500000000, // 2458905.500000000 = A.D. 2020-Feb-26 00:00:00.0000 TDB 
                //    M0 = 7.601340053048488, // Mean anomaly (mean value) 
                //    n = 6.125501503540979E+01, // Longitude rate(mean value)
                //    e = 1.434252567539511E-05, // 	Eccentricity (mean value)
                //    a = 2.371462440229666E-03, // Semimajor axis, in a.u.
                //    i = 1.568078930495086E+02, // inclination w.r.t XY-plane (degrees)
                //    omega0 = 2.256026385336725E+02, // Argument of Perifocus (degrees)                    
                //    node0 = 1.883768954266065E+02, // Longitude of Ascending Node, in degrees

                //    // from https://ssd.jpl.nasa.gov/?sat_elem
                //    Pw = 386.371 * 365.25, //  Argument of periapsis precession period (mean value)
                //    Pnode = 687.446 * 365.25, // Longitude of the ascending node precession period(mean value)
                //    RA = 299.456, // Right ascension and ...
                //    Dec = 43.414 // Declination of the Laplace plane pole with respect to the ICRF.
                //}, neptune);


                // Triton. This also works
                // (ecliptical plane is used)
                /*
                Ephemeris Type [change] : 	ELEMENTS
                Target Body [change] : 	Triton (NI) [801]
                Center [change] : 	Neptune (body center) [500@899]
                Time Span [change] : 	Start=2020-02-26, Stop=2020-02-27, Step=1 d
                Table Settings [change] : 	reference plane=BODY EQUATOR
                                            reference plane : ecliptic and mean equinox of reference epoch
                                            reference system: ICRF/J2000.0
                                            TP type: absolute
                Display/Output [change] : 	plain text
                 */



                return GenericPosition(jd, new Orbit()
                {
                    jd0 = 2458880.500000000, // = A.D. 2020 - Feb - 01 00:00:00.0000 TDB
                    M0 = 1.436799437841057E+01, // Mean anomaly (mean value) 
                    n = 6.125483834432863E+01, // Longitude rate(mean value)
                    e = 2.482410002230766E-05, // 	Eccentricity (mean value)
                    a = 2.371467000588459E-03, // Semimajor axis, in a.u.
                    i = 1.293222253304781E+02, // inclination w.r.t XY-plane (degrees)
                    omega0 = 1.371144230594285E+02, // Argument of Perifocus (degrees)                    
                    node0 = 2.210794005845409E+02, // Longitude of Ascending Node, in degrees

                    // from https://ssd.jpl.nasa.gov/?sat_elem
                    Pw = 386.371 * 365.25, //  Argument of periapsis precession period (mean value)
                    Pnode = 687.446 * 365.25, // Longitude of the ascending node precession period(mean value)
                }, neptune);

            }

            // Sao
            else if (index == 5)
            {


                // This works

                /*
                Ephemeris Type [change] : 	ELEMENTS
                Target Body [change] : 	Sao (2002N2) [811]
                Center [change] : 	Neptune (body center) [500@899]
                Time Span [change] : 	Start=2020-02-01, Stop=2020-03-21, Step=1 d
                Table Settings [change] : 	
                                     reference plane : ecliptic and mean equinox of reference epoch

                Display/Output [change] : 	plain text 
                */
                return GenericPosition(jd, new Orbit()
                {
                    jd0 = 2458880.500000000, // = A.D. 2020 - Feb - 01 00:00:00.0000 TDB
                    M0 = 1.996144109815055E+02, // Mean anomaly (mean value) 
                    n = 1.230714845743210E-01, // Longitude rate(mean value)
                    e = 1.335775826732659E-01, // 	Eccentricity (mean value)
                    a = 1.489277032908810E-01, // Semimajor axis, in a.u.
                    i = 5.265635714761137E+01, // inclination w.r.t XY-plane (degrees)
                    omega0 = 6.476950956637903E+01, // Argument of Perifocus (degrees)                    
                    node0 = 6.021474467002238E+01, // Longitude of Ascending Node, in degrees

                    // from https://ssd.jpl.nasa.gov/?sat_elem
                    Pw = 436880.78 * 365.25, //  Argument of periapsis precession period (mean value)
                    Pnode = 5382.39 * 365.25, // Longitude of the ascending node precession period(mean value)
                }, neptune);
            }
            else if (index == 6)
            {
                return SatellitePositionJPL(jd, neptune);
            }


            else
                throw new ArgumentException("Incorrect moon index", nameof(index));
        }

        /// <summary>
        /// Moon mean radius, in km
        /// </summary>
        private static double[] MOON_RADIUS = new double[] { 1354.0, 170.0, 170.0, 1354.0 };

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
            if (index == 1 || index == 2 || index == 3 || index == 4)
                return ToDegrees(Atan(MOON_RADIUS[index - 1] / (distance * 149597870.0))) * 3600;
            else
                return 1;
                //throw new ArgumentException("Incorrect moon index", nameof(index));
        }

        /// <summary>
        /// Absolute magnitudes
        /// </summary>
        private static double[] MOON_MAGNIUDE = new double[] { -1.24, 4.0, 4.0, -1.24 };

        public static float Magnitude(double Delta, double r, int index)
        {
            if (index == 1 || index == 2 || index == 3 || index == 4)
                return (float)(MOON_MAGNIUDE[index - 1] + 5 * Math.Log10(r * Delta));
            else
                return 1;//
                //throw new ArgumentException("Incorrect moon index", nameof(index));
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
            const double n = 360.0 / 360.1362; // Mean motion, in degrees per day
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

        class Orbit
        {
            /// <summary>
            /// Orbital elements epoch
            /// </summary>
            public double jd0 { get; set; }

            /// <summary>
            /// Mean anomaly at epoch, degrees
            /// </summary>
            public double M0 { get; set; }

            /// <summary>
            /// Mean motion, degrees/day  
            /// </summary>
            public double n { get; set; }

            /// <summary>
            /// Eccentricity
            /// </summary>
            public double e { get; set; }

            /// <summary>
            /// Semi-major axis, au
            /// </summary>
            public double a { get; set; }

            /// <summary>
            /// Inclination w.r.t XY-plane, degrees
            /// </summary>
            public double i { get; set; }

            /// <summary>
            /// Argument of perifocus, degrees
            /// </summary>
            public double omega0 { get; set; }

            /// <summary>
            /// Longitude of Ascending Node, degrees
            /// </summary>
            public double node0 { get; set; }

            /// <summary>
            /// Argument of periapsis precession period (mean value), years
            /// From https://ssd.jpl.nasa.gov/?sat_elem
            /// </summary>
            public double Pw { get; set; }

            /// <summary>
            /// Longitude of the ascending node precession period (mean value), years
            /// From https://ssd.jpl.nasa.gov/?sat_elem
            /// </summary>
            public double Pnode { get; set; }
        }

        private static CrdsEcliptical GenericPosition(double jd, Orbit orbit, CrdsEcliptical neptune)
        {
            NutationElements ne = Nutation.NutationElements(jd);
            double epsilon = Date.TrueObliquity(jd, ne.deltaEpsilon);

            // convert current coordinates to epoch, as algorithm requires
            CrdsEquatorial eq = neptune.ToEquatorial(epsilon);
            PrecessionalElements peEpoch = Precession.ElementsFK5(jd, Date.EPOCH_J2000);
            CrdsEquatorial eqNeptuneEpoch = Precession.GetEquatorialCoordinates(eq, peEpoch);

            // take light-time effect into account
            double tau = PlanetPositions.LightTimeEffect(neptune.Distance);

            double t = (jd - tau - orbit.jd0);

            double M = To360(orbit.M0 + orbit.n * t);

            double omega = To360(orbit.omega0 + t * 360.0 / orbit.Pw);
            double node = To360(orbit.node0 + t * 360.0 / orbit.Pnode);

            // Find eccentric anomaly by solving Kepler equation
            double E = SolveKepler(M, orbit.e);

            double X = orbit.a * (Cos(E) - orbit.e);
            double Y = orbit.a * Sqrt(1 - orbit.e * orbit.e) * Sin(E);

            // ecliptical pole
            CrdsEquatorial pole = new CrdsEcliptical(0, 90).ToEquatorial(epsilon);

            // cartesian state vector of satellite
            var d =
                Matrix.R2(ToRadians(-eqNeptuneEpoch.Delta)) *
                Matrix.R3(ToRadians(eqNeptuneEpoch.Alpha)) *
                Matrix.R3(ToRadians(-pole.Alpha - 90)) *
                Matrix.R1(ToRadians(pole.Delta - 90)) *
                Matrix.R3(ToRadians(-node)) *
                Matrix.R1(ToRadians(-orbit.i)) *
                Matrix.R3(ToRadians(-omega)) *
                new Matrix(new double[,] { { X / neptune.Distance }, { Y / neptune.Distance }, { 0 } });

            // radial component, positive away from observer
            // converted to degrees
            double x = ToDegrees(d.Values[0, 0]);

            // semimajor axis, expressed in degrees, as visible from Earth
            double theta = ToDegrees(Atan(orbit.a / neptune.Distance));

            // offsets values in degrees           
            double dAlphaCosDelta = ToDegrees(d.Values[1, 0]);
            double dDelta = ToDegrees(d.Values[2, 0]);

            double delta = eqNeptuneEpoch.Delta + dDelta;
            double dAlpha = dAlphaCosDelta / Cos(ToRadians(eqNeptuneEpoch.Delta));
            double alpha = eqNeptuneEpoch.Alpha + dAlpha;

            CrdsEquatorial eqSatelliteEpoch = new CrdsEquatorial(alpha, delta);

            // convert jd0 equatorial coordinates to current epoch
            // and to ecliptical
            PrecessionalElements pe = Precession.ElementsFK5(Date.EPOCH_J2000, jd);
            CrdsEquatorial eqSatellite = Precession.GetEquatorialCoordinates(eqSatelliteEpoch, pe);
            CrdsEcliptical eclSatellite = eqSatellite.ToEcliptical(epsilon);

            // calculate distance to Earth
            eclSatellite.Distance = neptune.Distance + x / theta * orbit.a;

            return eclSatellite;
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


        private class SatellitePositionData
        {
            public double Jd { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
        }

        private static List<SatellitePositionData> SatellitePositions = new List<SatellitePositionData>();

        private static void LoadJPLData()
        {
            if (IsInitialized) return;

            string line = "";

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"ADK.Data.Satellite.txt"))
            using (var sr = new StreamReader(stream))
            {
                while (line != null && !sr.EndOfStream)
                {
                    line = sr.ReadLine();

                    double jd = double.Parse(line.Substring(19, 17), CultureInfo.InvariantCulture);
                    double X = double.Parse(line.Substring(40, 10), CultureInfo.InvariantCulture);
                    double Y = double.Parse(line.Substring(49, 10), CultureInfo.InvariantCulture);

                    SatellitePositionData pd = new SatellitePositionData()
                    {
                        Jd = jd,
                        X = X,
                        Y = Y
                    };

                    SatellitePositions.Add(pd);
                }
            }

            IsInitialized = true;
        }

        private static bool IsInitialized = false;

        private static CrdsEcliptical SatellitePositionJPL(double jd, CrdsEcliptical planet)
        {
            LoadJPLData();

            var pos0 = SatellitePositions.LastOrDefault(np => np.Jd < jd);
            var pos1 = SatellitePositions.FirstOrDefault(np => np.Jd >= jd);

            if (pos0 != null && pos1 != null)
            {
                NutationElements ne = Nutation.NutationElements(jd);
                double epsilon = Date.TrueObliquity(jd, ne.deltaEpsilon);

                double dt = pos1.Jd - pos0.Jd;
                double x = pos0.X + (jd - pos0.Jd) / dt * (pos1.X - pos0.X);
                double y = pos0.Y + (jd - pos0.Jd) / dt * (pos1.Y - pos0.Y);

                CrdsEquatorial eqPlanet = planet.ToEquatorial(epsilon);


                // offsets values in degrees           
                double dAlphaCosDelta = x / 3600;
                double dDelta = y / 3600;

                double delta = eqPlanet.Delta + dDelta;
                double dAlpha = dAlphaCosDelta / Cos(ToRadians(eqPlanet.Delta));
                double alpha = eqPlanet.Alpha + dAlpha;

                var eqSatellite = new CrdsEquatorial(alpha, delta);

                CrdsEcliptical eclSatellite = eqSatellite.ToEcliptical(epsilon);
                eclSatellite.Distance = planet.Distance;

                return eclSatellite;
            }

            return null;



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

        /// <summary>
        /// Helper class to perform basic matrix operations
        /// </summary>
        /// <remarks>
        /// See info about rotation matrix: https://www.astro.rug.nl/software/kapteyn/celestialbackground.html
        /// </remarks>
        private class Matrix
        {
            /// <summary>
            /// Matrix values
            /// </summary>
            public double[,] Values { get; private set; }

            /// <summary>
            /// Creates new matrix from two-dimensional double array
            /// </summary>
            /// <param name="values"></param>
            public Matrix(double[,] values)
            {
                Values = values;
            }

            /// <summary>
            /// Multiplies two matrices
            /// </summary>
            /// <param name="A">Left operand</param>
            /// <param name="B">right operand</param>
            /// <returns>New matrix as a multiplication of left and right operands</returns>
            public static Matrix operator *(Matrix A, Matrix B)
            {
                int rA = A.Values.GetLength(0);
                int cA = A.Values.GetLength(1);
                int rB = B.Values.GetLength(0);
                int cB = B.Values.GetLength(1);
                double temp = 0;
                double[,] r = new double[rA, cB];
                if (cA != rB)
                {
                    throw new ArgumentException("Unable to multiply matrices");
                }
                else
                {
                    for (int i = 0; i < rA; i++)
                    {
                        for (int j = 0; j < cB; j++)
                        {
                            temp = 0;
                            for (int k = 0; k < cA; k++)
                            {
                                temp += A.Values[i, k] * B.Values[k, j];
                            }
                            r[i, j] = temp;
                        }
                    }
                    return new Matrix(r);
                }
            }

            /// <summary>
            /// Gets R1(a) rotation matrix 
            /// </summary>
            /// <param name="a">Angle of rotation, in radians</param>
            /// <returns>
            /// R1(a) rotation matrix
            /// </returns>
            public static Matrix R1(double a)
            {
                return new Matrix(
                    new double[3, 3] {
                        { 1, 0, 0 },
                        { 0, Cos(a), Sin(a) },
                        { 0, -Sin(a), Cos(a) }
                    });
            }

            /// <summary>
            /// Gets R2(a) rotation matrix 
            /// </summary>
            /// <param name="a">Angle of rotation, in radians</param>
            /// <returns>
            /// R2(a) rotation matrix
            /// </returns>
            public static Matrix R2(double a)
            {
                return new Matrix(
                    new double[3, 3] {
                        { Cos(a), 0, -Sin(a) },
                        { 0, 1, 0 },
                        { Sin(a), 0, Cos(a) }
                    });
            }

            /// <summary>
            /// Gets R3(a) rotation matrix 
            /// </summary>
            /// <param name="a">Angle of rotation, in radians</param>
            /// <returns>
            /// R3(a) rotation matrix
            /// </returns>
            public static Matrix R3(double a)
            {
                return new Matrix(
                    new double[3, 3] {
                        { Cos(a), Sin(a), 0 },
                        { -Sin(a), Cos(a), 0 },
                        { 0, 0, 1 }
                    });
            }
        }
    }
}
