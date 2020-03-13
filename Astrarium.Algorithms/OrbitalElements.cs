using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Algorithms
{
    /// <summary>
    /// Represents orbital elements of minor planet or comet
    /// </summary>
    public class OrbitalElements
    {
        /// <summary>
        ///  Epoch of the orbital elements (in Julian Days)
        /// </summary>
        public double Epoch { get; set; }

        /// <summary>
        /// Mean anomaly at the epoch, in degrees
        /// </summary>
        public double M { get; set; }

        /// <summary>
        /// Argument of perihelion, in degrees
        /// </summary>
        public double omega { get; set; }

        /// <summary>
        /// Longitude of the ascending node, in degrees
        /// </summary>
        public double Omega { get; set; }

        /// <summary>
        /// Inclination to the ecliptic, in degrees
        /// </summary>
        public double i { get; set; }

        /// <summary>
        /// Orbital eccentricity
        /// </summary>
        public double e { get; set; }

        /// <summary>
        /// Semimajor axis, AU
        /// </summary>
        public double a { get; set; }

        /// <summary>
        /// Perihelion distance, AU
        /// </summary>
        public double q { get; set; }

        public OrbitalElements() { }

        public OrbitalElements(OrbitalElements other)
        {
            Epoch = other.Epoch;
            M = other.M;
            omega = other.omega;
            Omega = other.Omega;
            i = other.i;
            e = other.e;
            a = other.a;
            q = other.q;
        }
    }
}
