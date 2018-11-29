using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Objects
{
    public class Planet : CelestialObject
    {
        /// <summary>
        /// Serial number of the planet, from 1 (Mercury) to 8 (Neptune).
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Heliocentrical coordinates of the planet.
        /// </summary>
        public CrdsHeliocentrical Heliocentrical { get; set; }

        /// <summary>
        /// Geocentrical equatorial coordinates
        /// </summary>
        public CrdsEquatorial Equatorial0 { get; set; } = new CrdsEquatorial();

        /// <summary>
        /// Apparent topocentrical equatorial coordinates
        /// </summary>
        public CrdsEquatorial Equatorial { get; set; }

        /// <summary>
        /// Ecliptical corrdinates
        /// </summary>
        public CrdsEcliptical Ecliptical { get; set; }

        /// <summary>
        /// Visible semidiameter, in seconds of arc
        /// </summary>
        public double Semidiameter { get; set; }

        /// <summary>
        /// Planet flattening. 0 means ideal sphere.
        /// </summary>
        public float Flattening { get; set; }

        public double Elongation { get; set; }

        public double Parallax { get; set; }

        public double PhaseAngle { get; set; }

        public double Phase { get; set; }

        /// <summary>
        /// Distance from planet to Sun
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// Magnitude of planet
        /// </summary>
        public float Magnitude { get; set; }

        /// <summary>
        /// Position angle of planet rotation axis.
        /// Measured counter-clockwise from direction to celestial north pole towards planet north pole.
        /// </summary>
        public double PAaxis { get; set; }

        /// <summary>
        /// Planetocentric declination of the Earth.
        /// If poisitive, the planet northern pole is tilted towards the Earth.
        /// Measured in degrees.
        /// </summary>
        public double D { get; set; }

        /// <summary>
        /// Planetographic longitude of central meridian, in degrees.
        /// </summary>
        public double CM { get; set; }

        public const int MERCURY = 1;
        public const int VENUS = 2;
        public const int EARTH = 3;
        public const int MARS = 4;
        public const int JUPITER = 5;
        public const int SATURN = 6;
        public const int URANUS = 7;
        public const int NEPTUNE = 8;
    }
}
