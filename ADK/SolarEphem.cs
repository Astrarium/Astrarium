using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK
{
    public static class SolarEphem
    {
        /// <summary>
        /// Calculates parallax of the Sun
        /// </summary>
        /// <param name="distance">Distance to the Sun, in A.U.</param>
        /// <returns>Parallax value in degrees.</returns>
        /// <remarks>
        /// Method is taken from AA(II), p. 279.
        /// </remarks>
        public static double Parallax(double distance)
        {
            return 8.794 / distance / 3600;
        }

        /// <summary>
        /// Gets solar semidiameter
        /// </summary>
        /// <param name="distance">Distance to the Sun, in AU.</param>
        /// <returns>
        /// Solar semidiameter, in arcseconds.
        /// </returns>
        /// <remarks>
        /// Formula is taken from AA(I) book, p. 359.
        /// </remarks>
        public static double Semidiameter(double distance)
        {
            return 959.63 / distance;
        }
    }
}
