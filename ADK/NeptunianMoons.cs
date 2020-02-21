using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using static ADK.Angle;

namespace ADK
{
    public static class NeptunianMoons
    {

        // Triton motion, Harris:
        // http://articles.adsabs.harvard.edu/cgi-bin/nph-iarticle_query?1984NASCP2330..357H&defaultprint=YES&filetype=.pdf

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jd"></param>
        /// <param name="eq">Geocentric coordinates of Neptune for epoch of date</param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static CrdsEcliptical TritonPosition(double jd, CrdsEcliptical neptune)
        {
            NutationElements ne = Nutation.NutationElements(jd);
            double epsilon = Date.TrueObliquity(jd, ne.deltaEpsilon);

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
            const double ap = 297.813;       // 1950.0 RA of the pole of the invariable plane
            const double dp = 41.185;        // 1950.0 Declination of the pole of the invariable plane


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

            PrecessionalElements pe = Precession.ElementsFK5(Date.EPOCH_J1950, jd);
            CrdsEquatorial eqTriton = Precession.GetEquatorialCoordinates(eqTriton1950, pe);

            CrdsEcliptical eclTriton = eqTriton.ToEcliptical(epsilon);

            eclTriton.Distance = neptune.Distance + x / theta * a;
            return eclTriton;
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
        }
    }
}
