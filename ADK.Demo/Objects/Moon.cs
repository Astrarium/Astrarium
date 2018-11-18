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

        public double PositionAngleBrightLimb { get; set; }

        public CrdsHorizontal HorizontalNorth { get; set; }
    }
}
