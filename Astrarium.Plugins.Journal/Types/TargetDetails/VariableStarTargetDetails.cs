using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    public class VariableStarTargetDetails : StarTargetDetails
    {
        /// <summary>
        /// Variable star type or subtype like Delta Cepheid, Mira, Eruptive, Semiregular, Supernovae
        /// </summary>
        public string VarStarType { get; set; }

        /// <summary>
        /// Maximal apparent magnitude. The derived <see cref="StarTargetDetails.Magnitude"/> will be used for minimal apparent magnitude
        /// </summary>
        public double? MaxMagnitude { get; set; }

        /// <summary>
        /// Pperiod of variable star (if any) in days 
        /// </summary>
        public double? Period { get; set; }
    }
}
