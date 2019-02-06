using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Objects
{
    public class Sun : SizeableCelestialObject, IMovingObject
    {
        /// <summary>
        /// Apparent topocentrical equatorial coordinates
        /// </summary>
        public CrdsEquatorial Equatorial { get; set; }

        public CrdsEcliptical Ecliptical { get; set; }

        /// <summary>
        /// Average daily motion of the Sun
        /// </summary>
        public double AverageDailyMotion => 0.985555;
    }
}
