using ADK;
using Planetarium.Types.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Objects
{
    public class JupiterMoon : PlanetMoon
    {
        public JupiterMoon(int number)
        {
            Number = number;
        }

        /// <summary>
        /// Apparent equatorial coordinates of the Galilean moon
        /// </summary>
        public CrdsEquatorial Equatorial { get; set; }

        /// <summary>
        /// Planetocentric rectangular coordinates of the Galilean moon
        /// </summary>
        public CrdsRectangular Rectangular { get; set; }

        /// <summary>
        /// Planetocentric rectangular coordinates of the Galilean moon, as seen from Sun
        /// </summary>
        public CrdsRectangular RectangularS { get; set; }

        /// <summary>
        /// Name of the Galilean moon
        /// </summary>
        public override string Name => Text.Get($"JupiterMoon.{Number}.Name");

        /// <summary>
        /// Name of moon shadow
        /// </summary>
        public string ShadowName => Text.Get($"JupiterMoon.{Number}.Shadow");

        /// <summary>
        /// Gets Galilean moon names
        /// </summary>
        public override string[] Names => new[] { Name };

        public override double DistanceFromEarth { get; internal set; }

        public override bool IsEclipsedByPlanet
        {
            get
            {
                return
                    RectangularS.Z > 0 && RectangularS.X * RectangularS.X + RectangularS.Y * RectangularS.Y * 1.14784224788 <= 1.1;
            }
        }
    }
}
