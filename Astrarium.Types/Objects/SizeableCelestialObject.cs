using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Types
{
    public abstract class SizeableCelestialObject : CelestialObject
    {
        /// <summary>
        /// Visible semidiameter, in seconds of arc
        /// </summary>
        public virtual double Semidiameter { get; set; }
    }
}
