using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK
{
    public static class Interpolation
    {
        public static double Lagrange(double[] x, double[] y, double x0)
        {
            if (x.Length != y.Length)
                throw new ArgumentException("Array sizes do not match.");

            if (x.Length < 2)
                throw new ArgumentException("Arrays must contains at least 2 points.");

            double product, sum = 0;

            int n = x.Length;

            product = 1;
            // Peforming Arithmatic Operation
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (j != i)
                    {
                        product *= (x0 - x[j]) / (x[i] - x[j]);
                    }
                }
                sum += product * y[i];

                product = 1;    // Must set to 1
            }

            return sum;
        }
    }
}
