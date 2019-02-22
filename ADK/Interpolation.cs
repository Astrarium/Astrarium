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

        public static void FindRoot(double[] x, double[] y, double epsilon, out double x0)
        {
            if (x.Length != y.Length)
                throw new ArgumentException("Array sizes do not match.");

            if (x.Length < 2)
                throw new ArgumentException("Arrays must contain at least 2 points.");

            // left edge of the segment
            double a = x[0];

            // right edge of the segment
            double b = x[x.Length - 1];

            // midpoint of the time segment
            x0 = (a + b) / 2;

            // "y0" is a value of interpolated function at the midpoint of the segment
            // "y2" is a value of interpolated function at the right edge of the segment
            double y0, y2;

            do
            {
                y0 = Lagrange(x, y, x0);
                y2 = Lagrange(x, y, b);

                // the function changes sign at the right half of the segment
                if (y0 * y2 < 0)
                {
                    // move left point of the segment
                    a = x0;
                }
                // the function changes sign at the left half of the segment
                else
                {
                    // move right point of the segment
                    b = x0;
                }

                // caculate new value of the midpoint
                x0 = (b + a) / 2;
            }
            while (Math.Abs(y0) > epsilon);
        }

        public static void FindMaximum(double[] x, double[] y, double epsilon, out double x0, out double y0)
        {
            FindExtremum(x, y, ExtremumType.Max, epsilon, out x0, out y0);
        }

        public static void FindMinimum(double[] x, double[] y, double epsilon, out double x0, out double y0)
        {
            FindExtremum(x, y, ExtremumType.Min, epsilon, out x0, out y0);
        }

        /// <summary>
        /// Finds local extremum of the function on a segment
        /// </summary>
        /// <param name="x">Argument values, defining segment points</param>
        /// <param name="y">Function values on the segment</param>
        /// <param name="extremum">Type of extremum (maximum or minimum)</param>
        /// <param name="epsilon">Expected accuracy</param>
        /// <param name="x0">Output value of X-coordinate of the extremum</param>
        /// <param name="y0">Output value of Y-coordinate of the extremum</param>
        private static void FindExtremum(double[] x, double[] y, ExtremumType extremum, double epsilon, out double x0, out double y0)
        {
            if (x.Length != y.Length)
                throw new ArgumentException("Array sizes do not match.");

            if (x.Length < 2)
                throw new ArgumentException("Arrays must contain at least 2 points.");

            // X-coordinate of extremum point 
            x0 = 0;

            // left edge of the segment
            double a = x[0];

            // right edge of the segment
            double b = x[x.Length - 1];

            // golden ratio
            double ratio = 1.61803399;

            do
            {
                double x1 = b - (b - a) / ratio;
                double x2 = a + (b - a) / ratio;
                double y1 = Lagrange(x, y, x1);
                double y2 = Lagrange(x, y, x2);

                if ((extremum == ExtremumType.Max && y1 <= y2) || 
                    (extremum == ExtremumType.Min && y1 >= y2))
                {
                    a = x1;
                }
                else
                {
                    b = x2;
                }
                x0 = (a + b) / 2;
            }
            while (Math.Abs(b - a) > epsilon);

            y0 = Lagrange(x, y, x0);
        }

        private enum ExtremumType
        {
            Max = 0,
            Min = 1
        }
    }
}
