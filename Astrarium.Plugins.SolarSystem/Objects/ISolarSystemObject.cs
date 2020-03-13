using Astrarium.Algorithms;
using Astrarium.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Objects
{
    public interface ISolarSystemObject
    {
        /// <summary>
        /// Distance from Earth, in AU
        /// </summary>
        double DistanceFromEarth { get; }
    }
}
