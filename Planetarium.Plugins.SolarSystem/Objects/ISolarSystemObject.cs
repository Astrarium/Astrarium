using ADK;
using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Objects
{
    public interface ISolarSystemObject
    {
        /// <summary>
        /// Distance from Earth, in AU
        /// </summary>
        double DistanceFromEarth { get; }
    }
}
