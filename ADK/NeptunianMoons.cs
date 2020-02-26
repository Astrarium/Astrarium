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
        public static CrdsEcliptical Position(double jd, CrdsEcliptical neptune, int index)
        {
            if (index == 1)
                return TritonPosition(jd, neptune);
            else if (index == 2)
                return NereidPosition(jd, neptune);
            else if (index == 3)
                return NereidPositionJPL(jd, neptune);
            else
                throw new ArgumentException("Incorrect moon index");
        }

        /// <summary>
        /// 1 a.u. (astronomical unit) in km
        /// </summary>
        private const double AU = 149597870;

        // Triton motion, Harris:
        // http://articles.adsabs.harvard.edu/cgi-bin/nph-iarticle_query?1984NASCP2330..357H&defaultprint=YES&filetype=.pdf

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jd"></param>
        /// <param name="eq">Geocentric coordinates of Neptune for epoch of date</param>
        /// <param name="distance"></param>
        /// <returns></returns>
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

            const double n = 61.2588532;     // nodal mean motiom, degrees per day
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
                new Matrix(new [,] { { a * Cos(ToRadians(lambda)) }, { a * Sin(ToRadians(lambda)) }, { 0 } });

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
            double dAlpha = dAlphaCosDelta / Cos(ToRadians(delta));
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
        /// Gets visible semidiameter of Triton, in seconds of arc 
        /// </summary>
        /// <param name="distance">Distance from Earth, in a.u.</param>
        /// <returns>
        /// Visible semidiameter of Triton, in seconds of arc
        /// </returns>
        public static double TritonSemidiameter(double distance) 
        {
            return ToDegrees(Atan(1354.0 / (distance * AU))) * 3600;
        }

        private class NereidPositionData
        {
            public double Jd { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
        }

        private static List<NereidPositionData> NereidPositions = new List<NereidPositionData>();

        private static void LoadJPLData()
        {
            if (IsInitialized) return;

            string line = "";

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"ADK.Data.Nereid"))
            using (var sr = new StreamReader(stream))
            {
                while (line != null && !sr.EndOfStream)
                {
                    line = sr.ReadLine();

                    double jd = double.Parse(line.Substring(19, 17), CultureInfo.InvariantCulture);
                    double X = double.Parse(line.Substring(62, 8), CultureInfo.InvariantCulture);
                    double Y = double.Parse(line.Substring(71, 8), CultureInfo.InvariantCulture);

                    NereidPositionData pd = new NereidPositionData()
                    {
                        Jd = jd,
                        X = X,
                        Y = Y
                    };

                    NereidPositions.Add(pd);
                }
            }

            IsInitialized = true;
        }

        private static bool IsInitialized = false;

        private static CrdsEcliptical NereidPositionJPL(double jd, CrdsEcliptical neptune)
        {
            LoadJPLData();

            var pos = NereidPositions.FirstOrDefault(np => Abs(np.Jd - jd) < 1);

            if (pos != null)
            {
                NutationElements ne = Nutation.NutationElements(jd);
                double epsilon = Date.TrueObliquity(jd, ne.deltaEpsilon);

                
                CrdsEquatorial eqNeptune = neptune.ToEquatorial(epsilon);
               

                // offsets values in degrees           
                double dAlphaCosDelta = pos.X / 3600;
                double dDelta = pos.Y / 3600;

                double delta = eqNeptune.Delta + dDelta;
                double dAlpha = dAlphaCosDelta / Cos(ToRadians(eqNeptune.Delta));
                double alpha = eqNeptune.Alpha + dAlpha;

                var eqNereid = new CrdsEquatorial(alpha, delta);

                CrdsEcliptical eclNereid = eqNereid.ToEcliptical(epsilon);
                eclNereid.Distance = neptune.Distance;

                return eclNereid;
            }

            return null;



        }

        // http://adsabs.harvard.edu/full/1981AJ.....86.1728M
        private static CrdsEcliptical NereidPosition(double jd, CrdsEcliptical neptune)
        {
            NutationElements ne = Nutation.NutationElements(jd);
            double epsilon = Date.TrueObliquity(jd, ne.deltaEpsilon);

            // convert current coordinates to J1950 epoch, as algorithm requires
            CrdsEquatorial eq = neptune.ToEquatorial(epsilon);
            PrecessionalElements pe1950 = Precession.ElementsFK5(jd, Date.EPOCH_J1950);
            CrdsEquatorial eqNeptune1950 = Precession.GetEquatorialCoordinates(eq, pe1950);

            const double a = 0.036868;
            const double e0 = 0.74515;
            const double I0 = 10.041;
            const double Omega0 = 329.3;
            const double psi0 = 282.9; // omega0
            const double M0 = 358.91;
            const double n = 0.999552;

            const double OmegaN = 3.552;
            const double gamma = 22.313;

            const double jd0 = 2433680.5; // epoch: 3.0 Feb 1951


            // take light-time effect into account
            double tau = PlanetPositions.LightTimeEffect(neptune.Distance);

            double t = jd - tau - jd0; // in days
            double T = t / 36525.0; // in Julian centuries

            double psi = ToRadians(To360(psi0 + 2.68 * T));
            double twoTheta = ToRadians(To360(107.4 + 0.01196 * t));

            Func<double, double> omegaEquation = (om) => To360((psi0 + 2.68 * T - 19.25 * Sin(2 * psi) + 3.23 * Sin(4 * psi) - 0.725 * Sin(6 * psi) - 0.351 * Sin(twoTheta) - 0.7 * Sin(ToRadians(2 * om) - twoTheta))) - om;

            double omega = FindRoots(omegaEquation, 0, 360, 1e-8);
            omega = ToRadians(omega);

            double Omega = Omega0 - 2.4 * T + 19.7 * Sin(2 * psi) - 3.3 * Sin(4 * psi) + 0.7 * Sin(6 * psi) + 0.357 * Sin(twoTheta) + 0.276 * Sin(2 * omega - twoTheta);

            double e = e0 - 0.006 * Cos(2 * psi) + 0.0056 * Cos(2 * omega - twoTheta);

            double M = To360(M0 + n * t - 0.38 * Sin(2 * psi) + 1.0 * Sin(2 * omega - twoTheta));

            double cosI = Cos(ToRadians(I0)) - 9.4e-3 * Cos(2 * psi);
            double I = Acos(cosI);

            // eccentric anomaly
            double E = ToRadians(SolveKepler(M, e));

            double X = a * (Cos(E) - e);
            double Y = a * Sqrt(1 - e * e) * Sin(E);

            Matrix d =
                Matrix.Ry(ToRadians(-eqNeptune1950.Delta)) *
                Matrix.Rz(ToRadians(eqNeptune1950.Alpha)) *
                Matrix.Rz(ToRadians(OmegaN)) *
                Matrix.Rx(ToRadians(-gamma)) *
                Matrix.Rz(ToRadians(-Omega)) *
                Matrix.Rx(-I) *
                Matrix.Rz(-omega) *
                new Matrix(new double[,] { { X / neptune.Distance }, { Y / neptune.Distance }, { 0 } });

            // radial component, positive away from observer
            // converted to degrees
            double x = ToDegrees(d.Values[0, 0]);

            // offsets values in degrees           
            double dAlphaCosDelta = ToDegrees(d.Values[1, 0]);
            double dDelta = ToDegrees(d.Values[2, 0]);

            double delta = eqNeptune1950.Delta + dDelta;
            double dAlpha = dAlphaCosDelta / Cos(ToRadians(delta));
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
            return ToDegrees(E1);
        }

        private static double FindRoots(Func<double, double> func, double a, double b, double eps)
        {
            double dx;
            while (b - a > eps)
            {
                dx = (b - a) / 2;
                double xi = a + dx;

                if (func(a) * func(xi) < 0)
                {
                    b = xi;
                }
                else
                {
                    a = xi;
                }
            }

            
            return (b - a < eps) ? (a + b) / 2 : FindRoots(func, a, b, eps);
        }

        /// <summary>
        /// Helper class to perform basic matrix operations
        /// </summary>
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
            /// <remarks>
            /// See info about rotation matrix: https://www.astro.rug.nl/software/kapteyn/celestialbackground.html
            /// </remarks>
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
            /// <remarks>
            /// See info about rotation matrix: https://www.astro.rug.nl/software/kapteyn/celestialbackground.html
            /// </remarks>
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
            /// <remarks>
            /// See info about rotation matrix: https://www.astro.rug.nl/software/kapteyn/celestialbackground.html
            /// </remarks>
            public static Matrix R3(double a)
            {
                return new Matrix(
                    new double[3, 3] {
                        { Cos(a), Sin(a), 0 },
                        { -Sin(a), Cos(a), 0 },
                        { 0, 0, 1 }
                    });
            }

            public static Matrix Rx(double a)
            {
                return new Matrix(
                    new double[3, 3] {
                        { 1, 0, 0 },
                        { 0, Cos(a), Sin(a) },
                        { 0, -Sin(a), Cos(a) }
                    });
            }

            public static Matrix Ry(double a)
            {
                return new Matrix(
                    new double[3, 3] {
                        { Cos(a), 0, -Sin(a) },
                        { 0, 1, 0 },
                        { Sin(a), 0, Cos(a) }
                    });
            }

            public static Matrix Rz(double a)
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
