using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK
{
    public class GenericSatelliteOrbit
    {
        /// <summary>
        /// Orbital elements epoch
        /// </summary>
        public double jd0 { get; set; }

        /// <summary>
        /// Mean anomaly at epoch, degrees
        /// </summary>
        public double M0 { get; set; }

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
        /// Argument of perifocus, degrees
        /// </summary>
        public double omega0 { get; set; }

        /// <summary>
        /// Longitude of Ascending Node, degrees
        /// </summary>
        public double node0 { get; set; }

        /// <summary>
        /// Argument of periapsis precession period (mean value), years
        /// From https://ssd.jpl.nasa.gov/?sat_elem
        /// </summary>
        public double Pw { get; set; }

        /// <summary>
        /// Longitude of the ascending node precession period (mean value), years
        /// From https://ssd.jpl.nasa.gov/?sat_elem
        /// </summary>
        public double Pnode { get; set; }
    }
}
