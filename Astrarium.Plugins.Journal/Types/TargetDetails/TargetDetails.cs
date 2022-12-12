using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    public class TargetDetails
    {
        public double? RA { get; set; }
        public double? Dec { get; set; }
        public double? Alt { get; set; }
        public double? Azi { get; set; }
        public string Constellation { get; set; }
    }
}
