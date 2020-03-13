using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Algorithms
{
    public class GenericSatelliteOrbit
    {
        /// <summary>
        /// Orbital elements epoch
        /// </summary>
        public double jd { get; set; }

        /// <summary>
        /// Mean anomaly at epoch, degrees
        /// </summary>
        public double M { get; set; }

        /// <summary>
        /// Mean motion, degrees/day  
        /// </summary>
        public double n { get; set; }

        /// <summary>
        /// Eccentricity
        /// </summary>
        public double e { get; set; }

        /// <summary>
        /// Semi-major axis, au
        /// </summary>
        public double a { get; set; }

        /// <summary>
        /// Inclination w.r.t XY-plane, degrees
        /// </summary>
        public double i { get; set; }

        /// <summary>
        /// Argument of periapsis at epoch, degrees
        /// </summary>
        public double w { get; set; }

        /// <summary>
        /// Longitude of the ascending node at epoch, degrees
        /// </summary>
        public double Om { get; set; }

        /// <summary>
        /// Argument of periapsis precession period, years
        /// </summary>
        public double Pw { get; set; }

        /// <summary>
        /// Longitude of the ascending node precession period, years
        /// </summary>
        public double POm { get; set; }
    }
}
