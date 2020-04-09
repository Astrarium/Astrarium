using Astrarium.Algorithms;
using Astrarium.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.SolarSystem.Objects
{
    public interface ISolarSystemObject
    {
        /// <summary>
        /// Distance from Earth, in AU
        /// </summary>
        double DistanceFromEarth { get; }
    }
}
