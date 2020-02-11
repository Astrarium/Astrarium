using ADK;
using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Objects
{
    public abstract class SolarSystemObject : SizeableCelestialObject
    {
        /// <summary>
        /// Distance from Earth, in AU
        /// </summary>
        public abstract double DistanceFromEarth { get; internal set; }
    }
}
