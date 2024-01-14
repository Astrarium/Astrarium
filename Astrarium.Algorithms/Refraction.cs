using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Algorithms
{
    /// <summary>
    /// Contains methods for calculation of atmospheric refraction.
    /// </summary>
    public static class Refraction
    {
        /// <summary>
        /// Calculates refraction correction (in degrees) to be added to true object altitude 
        /// to obtain visible altitude
        /// </summary>
        /// <param name="h0"></param>
        /// <returns></returns>
        /// <remarks>See AA(II), page 106, formula 16.4</remarks>
        public static double CorrectionForVisibleCoordinates(double h, double P = 1010, double T = 10)
        {
            h = Math.Max(-1, h);
            double f = h + 10.3 / (h + 5.11);
            double ptc = (P / 1010 * 283 / (273 + T)) / 60.0;
            return ptc * (1.02 / Math.Tan(Angle.ToRadians(f)));
        }

        public static double CorrectionForTrueCoordinates(double h0, double P = 1010, double T = 10)
        {
            h0 = Math.Max(-1, h0);
            double f = h0 + 7.31 / (h0 + 4.4);
            double ptc = (P / 1010 * 283 / (273 + T)) / 60.0;
            double r  = ptc * (1.0 / Math.Tan(Angle.ToRadians(f)));

            double h = h0 - r;
            double delta = h0 - (h + CorrectionForVisibleCoordinates(h, P, T));
            return r - delta;
        }

        public static double Flattening(double h, double P = 1010, double T = 10)
        {
            const double diameter = 0.001;
            double horU = h + diameter / 2;
            double horD = h - diameter / 2;
            horU += CorrectionForVisibleCoordinates(horU, P, T);
            horD += CorrectionForVisibleCoordinates(horD, P, T);
            return (horU - horD) / diameter;
        }
    }
}
