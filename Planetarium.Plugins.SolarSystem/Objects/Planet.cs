using ADK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Objects
{
    public class Planet : SizeableCelestialObject, IMovingObject
    {
        /// <summary>
        /// Serial number of the planet, from 1 (Mercury) to 8 (Neptune).
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Planet name
        /// </summary>
        public string Name { get; set; }

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
        /// Planet flattening. 0 means ideal sphere.
        /// </summary>
        public float Flattening { get; set; }

        public double Elongation { get; set; }

        public double Phase { get; set; }

        /// <summary>
        /// Magnitude of planet
        /// </summary>
        public float Magnitude { get; set; }

        public double DistanceFromSun { get; set; }

        /// <summary>
        /// Gets planet names
        /// </summary>
        public override string[] Names => new[] { Name };

        /// <summary>
        /// Planet appearance parameters
        /// </summary>
        public PlanetAppearance Appearance { get; set; }

        /// <summary>
        /// Mean daily motion of the planet, in degrees
        /// </summary>
        public double AverageDailyMotion => DAILY_MOTIONS[Number - 1];

        /// <summary>
        /// Average daily motions of planets
        /// </summary>
        public static readonly double[] DAILY_MOTIONS = new[] { 1.3833, 1.2, 0, 0.542, 0.0831, 0.0336, 0.026666, 0.006668 };

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
