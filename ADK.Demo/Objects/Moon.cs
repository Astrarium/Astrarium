using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Objects
{
    public class Moon : CelestialObject
    {
        /// <summary>
        /// Geocentrical equatorial coordinates
        /// </summary>
        public CrdsEquatorial Equatorial0 { get; set; } = new CrdsEquatorial();

        /// <summary>
        /// Apparent topocentrical equatorial coordinates
        /// </summary>
        public CrdsEquatorial Equatorial { get; set; }

        public CrdsEcliptical Ecliptical { get; set; }

        public double Semidiameter { get; set; }

        public double Elongation { get; set; }

        public double Parallax { get; set; }

        public double PhaseAngle { get; set; }

        public double Phase { get; set; }

        /// <summary>
        /// Position angle of bright limb, in degrees.
        /// Measured counter-clockwise from direction to celestial North pole.
        /// Also known as χ (chi).
        /// </summary>
        public double PAlimb { get; set; }

        /// <summary>
        /// Position angle of North cusp, in degrees.
        /// Measured counter-clockwise from direction to celestial North pole.
        /// </summary>
        public double PAcusp { get; set; }

        /// <summary>
        /// Position angle of Moon axis, in degrees.
        /// </summary>
        public double PAaxis { get; set; }
    }
}
