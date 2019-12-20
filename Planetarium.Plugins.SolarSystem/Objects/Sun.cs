using ADK;
using Planetarium.Types.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Objects
{
    public class Sun : SizeableCelestialObject, IMovingObject
    {
        /// <summary>
        /// Apparent topocentrical equatorial coordinates
        /// </summary>
        public CrdsEquatorial Equatorial { get; set; }

        /// <summary>
        /// Ecliptical coordinates
        /// </summary>
        public CrdsEcliptical Ecliptical { get; set; }

        /// <summary>
        /// Average daily motion of the Sun
        /// </summary>
        public double AverageDailyMotion => 0.985555;

        /// <summary>
        /// Gets Sun names
        /// </summary>
        public override string[] Names => new[] { Name };

        /// <summary>
        /// Primary name
        /// </summary>
        public string Name => Text.Get("Sun.Name");
    }
}
