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
        /// Ecliptical coordinates
        /// </summary>
        public CrdsEcliptical Ecliptical { get; set; }
    }
}
