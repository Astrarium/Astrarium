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
        /// Calculates refraction correction (in degrees) to be added to true object altitude to obtain visible altitude
        /// </summary>
        /// <param name="h">True (geometric) altitude</param>
        /// <returns>Correction in degrees</returns>
        /// <remarks>See AA(II), page 106, formula 16.4</remarks>
        public static double CorrectionForVisibleCoordinates(double h, double P = 1010, double T = 10)
        {
            h = Math.Max(-1, h);
            double f = h + 10.3 / (h + 5.11);
            double ptc = (P / 1010 * 283 / (273 + T)) / 60.0;
            return ptc * (1.02 / Math.Tan(Angle.ToRadians(f)));
        }

        /// <summary>
        /// Calculates refraction correction (in degrees) to be subtracted from visual object altitude to obtain real (geometric) altitude
        /// </summary>
        /// <param name="h0">Visual altitude</param>
        /// <returns>Correction in degrees</returns>
        /// <remarks>See AA(II), page 106, formula 16.3</remarks>
        public static double CorrectionForTrueCoordinates(double h0, double P = 1010, double T = 10)
        {
            h0 = Math.Max(-1, h0);
         
            // use reverse refraction formula by Meeus
            
            double f = h0 + 7.31 / (h0 + 4.4);
            double ptc = (P / 1010 * 283 / (273 + T)) / 60.0;

            // non-corrected value of refraction (Meeus)
            double r  = ptc * (1.0 / Math.Tan(Angle.ToRadians(f)));
            
            double delta0;
            double delta = 0;

            // we need to use iterative procedure
            // to find accurate reverse refraction, because
            // direct and reverse refraction transformations do not match.
            // i.e:
            //
            // h - true geometric altitude
            // rv - refraction correction to obtain visible altitude (Meeus)
            // h0 = h + rv - visible altitude
            // rt - refraction correction to obtain true altitude (Meeus)
            // h' = h - rv - true altitude, reverse calculated
            //
            // So we have: h != h'

            do
            {
                delta0 = delta;

                // assumed true altitude on this step
                double ht = h0 - r;

                // "computed" visible altitude (reverse calculated)
                double hv = ht + CorrectionForVisibleCoordinates(ht, P, T);

                // difference between "real" and "computed" visible altitudes
                delta = h0 - hv;

                // corrected refraction
                r -= delta;
            }
            while (Math.Abs(delta - delta0) > 1e-6);

            return r;
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
