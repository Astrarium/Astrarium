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
        public static double Correction(double h, double P = 1010, double T = 10)
        {
            return 1.02 / Math.Tan(Angle.ToRadians(h + 10.3 / (h + 5.11))) / 60.0 * (P / 1010 * 283 / (273 + T));
        }

        public static double Flattening(double h, double diameter, double P = 1010, double T = 10)
        {
            // true altitude of upper limb
            double h_u = h + diameter / 2;

            // true altitude of lower limb
            double h_l = h - diameter / 2;

            // refraction correction for upper limb
            double R_u = Correction(h_u, P, T);

            // refraction correction for lower limb
            double R_l = Correction(h_l, P, T);

            // flattening
            return (h_u + R_u - (h_l + R_l)) / diameter;
        }

        //public static CrdsEquatorial CorrectForRefraction(CrdsHorizontal hor, CrdsGeographical geo, double theta0)
        //{
        //    hor.Altitude += Correction(hor.Altitude);
        //    return hor.ToEquatorial(geo, theta0);
        //}
    }
}
