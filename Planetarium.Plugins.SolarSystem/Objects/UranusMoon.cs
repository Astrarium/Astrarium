using ADK;
using Planetarium.Types.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Objects
{
    public class UranusMoon : SizeableCelestialObject
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
        public string Name => Text.Get($"UranusMoon.{Number}.Name");

        /// <summary>
        /// Number of the moon (1 to 5)
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// Apparent magnitude
        /// </summary>
        public float Magnitude { get; set; }

        /// <summary>
        /// Gets moon names
        /// </summary>
        public override string[] Names => new[] { Name };
    }
}
