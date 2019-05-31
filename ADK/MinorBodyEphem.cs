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
    }
}
