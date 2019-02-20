using System;

namespace ADK
{
    public static class Interpolation
    {
        public static double Lagrange(double[] x, double[] y, double x0)
        {
            if (x.Length != y.Length)
                throw new ArgumentException("Array sizes do not match.");

            if (x.Length < 2)
                throw new ArgumentException("Arrays must contain at least 2 points.");

            double sum = 0;
            int n = x.Length;
            for (int i = 0; i < n; i++)
            {
                double p = 1;
                for (int j = 0; j < n; j++)
                {
                    if (j != i)
                    {
                        p *= (x0 - x[j]) / (x[i] - x[j]);
                    }
                }
                sum += p * y[i];
            }

            return sum;
        }

        public static double ThreeTabular(double[] x, double[] y, double n)
        {
            double a = y[1] - y[0];
            double b = y[2] - y[1];
            double c = b - a;
            return y[1] + n / 2 * (a + b + n * c);
        }

        /// <summary>
        /// Searches for parabola vertex point
        /// </summary>
        /// <param name="x">Array of x-coordinates of a parabolic function (3 points)</param>
        /// <param name="y">Array of y-coordinates of a parabolic function (3 points)</param>
        /// <param name="xv">Output value of x-coordinate of the vertex</param>
        /// <param name="yv">Output value of y-coordinate of the vertex</param>
        // TODO: tests, add check for arrays ranges.
        public static bool FindParabolaVertex(double[] x, double[] y, out double xv, out double yv)
        {
            double denom = (x[0] - x[1]) * (x[0] - x[2]) * (x[1] - x[2]);
            double A = (x[2] * (y[1] - y[0]) + x[1] * (y[0] - y[2]) + x[0] * (y[2] - y[1])) / denom;
            double B = (x[2] * x[2] * (y[0] - y[1]) + x[1] * x[1] * (y[2] - y[0]) + x[0] * x[0] * (y[1] - y[2])) / denom;
            double C = (x[1] * x[2] * (x[1] - x[2]) * y[0] + x[2] * x[0] * (x[2] - x[0]) * y[1] + x[0] * x[1] * (x[0] - x[1]) * y[2]) / denom;

            xv = -B / (2 * A);
            yv = C - B * B / (4 * A);

            return xv >= x[0] && xv < x[2];
        }
    }
}
