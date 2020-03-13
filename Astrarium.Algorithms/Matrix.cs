using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using static Astrarium.Algorithms.Angle;

namespace Astrarium.Algorithms
{
    /// <summary>
    /// Helper class to perform basic matrix operations
    /// </summary>
    /// <remarks>
    /// See info about rotation matrix: https://www.astro.rug.nl/software/kapteyn/celestialbackground.html
    /// </remarks>
    internal class Matrix
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
