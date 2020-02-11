using ADK;
using Planetarium.Types.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Objects
{
    public class UranusMoon : PlanetMoon
    {
        public UranusMoon(int number)
        {
            Number = number;
        }

        /// <summary>
        /// Apparent equatorial coordinates of the moon
        /// </summary>
        public CrdsEquatorial Equatorial { get; set; }

        /// <summary>
        /// Planetocentric rectangular coordinates of the moon
        /// </summary>
        public CrdsRectangular Rectangular { get; set; }

        /// <summary>
        /// Name of the moon
        /// </summary>
        public override string Name => Text.Get($"UranusMoon.{Number}.Name");

        /// <summary>
        /// Gets moon names
        /// </summary>
        public override string[] Names => new[] { Name };

        public override double DistanceFromEarth { get; internal set; }

        public override bool IsEclipsedByPlanet => false;
    }
}
