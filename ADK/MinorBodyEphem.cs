using System;
using static System.Math;
using static ADK.Angle;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK
{
    public static class MinorBodyEphem
    {
        /// <summary>
        /// 1 a.u. (astronomical unit) in km
        /// </summary>
        private const double AU = 149597870;

        /// <summary>
        /// Calculates visible magnitude of the asteroid
        /// </summary>
        /// <param name="G">Phase coefficient of the asteroid</param>
        /// <param name="H">Absolute magnitude of the asteroid</param>
        /// <param name="beta">Phase angle of the asteroid</param>
        /// <param name="r">Distance from asteroid to the Sun (in a.u.)</param>
        /// <param name="delta">Distance from asteroid to the Earth (in a.u.)</param>
        /// <returns>Returns visible magnitude of the asteroid</returns>
        // TODO: tests
        public static float Magnitude(double G,
                                    double H,
                                    double beta,
                                    double r,
                                    double delta)
        {
            if (beta > 120)
            {
                // Meeus, Astronomical Formulae for Calculators (Russian translation), p. 104
                return (float)(H + 5 * Tan(r * delta) + G * beta);
            }
            else
            {
                // Meeus, Astronomical algorithms, p. 217
                double F1 = Exp(-3.33 * Pow(Tan(ToRadians(beta / 2)), 0.63));
                double F2 = Exp(-1.87 * Pow(Tan(ToRadians(beta / 2)), 1.22));

                return (float)(H + 5 * Log10(r * delta) - 2.5 * Log10((1 - G) * F1 + G * F2));
            }
        }

        // TODO: tests
        public static double PhaseAngle(double r, double delta, double R)
        {
            return ToDegrees(Acos((r * r + delta * delta - R * R) / (2 * r * delta)));
        }

        /// <summary>
        /// Gets semidiameter of asteroid in seconds of arc
        /// </summary>
        /// <param name="r">Distance Earth-asteroid, in a.u.</param>
        /// <param name="d">Diameter of asteroid, in km</param>
        /// <returns></returns>
        public static double Semidiameter(double r, double d)
        {
            if (d == 0)
                return 0;
            else
                return ToDegrees(Atan2(d / 2, r * AU)) * 3600;
        }

        /// <summary>
        /// Calculates details of comet appearance (tail length and coma diameter)
        /// </summary>
        /// <param name="H">Absolute magnitude of the comet</param>
        /// <param name="K">Phase coefficient of the comet</param>
        /// <param name="r">Distance from comet to the Sun (in a.u.)</param>
        /// <param name="delta">Distance from comet to the Earth (in a.u.)</param>
        /// <returns>Returns details of comet appearance</returns>
        /// <remarks>
        /// See https://www.projectpluto.com/update7b.htm#comet_tail_formula
        /// also see book : Hunting and Imaging Comets, M.Mobberley, pages 258-259
        /// </remarks>
        // TODO: tests
        public static CometAppearance CometAppearance(double H,
                                                    double K,
                                                    double r,
                                                    double delta)
        {
            double mhelio = H + K * Log10(r);
            double log10Lo = -0.0075 * mhelio * mhelio - 0.19 * mhelio + 2.10;
            double log10Do = -0.0033 * mhelio * mhelio - 0.07 * mhelio + 3.25;
            double Lo = Pow(10, log10Lo);
            double Do = Pow(10, log10Do);

            // tail length, in millions of kilometers
            double L = Lo * (1 - Pow(10, -4 * r)) * (1 - Pow(10, -2 * r));
            
            // coma diameter, in thousands of kilometers
            double D = Do * (1 - Pow(10, -2 * r)) * (1 - Pow(10, -r));
            
            // translate sizes to a.u.
            D = D * 1e3 / AU;
            L = L * 1e6 / AU;

            return new CometAppearance()
            {
                Coma = (float)ToDegrees(Atan(D / (2 * delta))) * 3600,
                Tail = (float)L
            };
        }
    }
}
