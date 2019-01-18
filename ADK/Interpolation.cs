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
    }
}
