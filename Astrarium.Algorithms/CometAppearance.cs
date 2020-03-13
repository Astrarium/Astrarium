using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Algorithms
{
    /// <summary>
    /// Describes appearance details of a comet
    /// </summary>
    public class CometAppearance
    {
        /// <summary>
        /// Comet tail length, in a. u.
        /// </summary>
        public float Tail { get; set; }

        /// <summary>
        /// Coma semidiameter, in arcseconds
        /// </summary>
        public float Coma { get; set; }
    }
}
