using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK
{
    public static class GalileanMoons
    {
        /// <summary>
        /// 1 a.u. (astronomical unit) in km
        /// </summary>
        private const double AU = 149597870;

        /// <summary>
        /// Moons radius, in km
        /// </summary>
        private static readonly double[] MR = { 3643.0 / 2, 3122.0 / 2, 5262.0 / 2, 4821.0 / 2 };

        /// <summary>
        /// Moons orbits semi-major axis, in km
        /// </summary>
        private static readonly double[] A = { 421700, 670900, 1070400, 1882700 };

        /// <summary>
        /// Jupiter radius, in km
        /// </summary>
        private const double JR = 71492;

        /// <summary>
        /// Solar radius, in km 
        /// </summary>
        private const double SR = 6.955e5;

        /// <summary>
        /// Gets semidiameter of Galilean moon shadow in units of Jupiter equatorial radii 
        /// </summary>
        /// <param name="R">Distance Sun-Jupiter centers, in a.u.</param>
        /// <param name="i">Galilean moon index, from 0 (Io) to 3 (Callisto)</param>
        /// <returns></returns>
        public static double ShadowSemidiameter(double R, int i)
        {
            // moon mean distances from Sun to galilean moons
            double x = R * AU - A[i];

            // distance moon - Jupiter surface
            double y = A[i] - JR;

            double z = (MR[i] * x) / (SR - MR[i]) - y;
            double d = (MR[i] * z) / (y + z);

            return d / JR;           
        }

        /// <summary>
        /// Gets semidiameter of Galilean moon in units of Jupiter equatorial radii 
        /// </summary>
        public static double MoonSemidiameter(double distance, int i)
        {
            // TODO
            return 0;
        }
    }
}
