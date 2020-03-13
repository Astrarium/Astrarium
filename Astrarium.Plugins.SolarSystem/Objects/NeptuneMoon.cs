using Astrarium.Algorithms;
using Astrarium.Objects;
using Astrarium.Types.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Objects
{
    public class NeptuneMoon : SizeableCelestialObject, IPlanetMoon, ISolarSystemObject, IMovingObject
    {
        public NeptuneMoon(int number)
        {
            Number = number;
        }

        /// <summary>
        /// Moon index, 1-based
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// Apparent equatorial coordinates of the moon
        /// </summary>
        public CrdsEquatorial Equatorial { get; set; }

        /// <summary>
        /// Planetocentric rectangular coordinates of the moon
        /// </summary>
        public CrdsRectangular Rectangular { get; set; }

        /// <summary>
        /// Longitude of central meridian, not used
        /// </summary>
        public double CM => 0;

        /// <summary>
        /// Apparent magnitude
        /// </summary>
        public float Magnitude { get; internal set; }

        /// <summary>
        /// Name of the moon
        /// </summary>
        public string Name => Text.Get($"NeptuneMoon.{Number}.Name");

        /// <summary>
        /// Gets moon names
        /// </summary>
        public override string[] Names => new[] { Name };

        public double DistanceFromEarth { get; internal set; }

        public bool IsEclipsedByPlanet => false;

        public double AverageDailyMotion => 0.006668;
    }
}
