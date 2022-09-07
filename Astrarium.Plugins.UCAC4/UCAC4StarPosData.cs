using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.UCAC4
{
    internal class UCAC4StarPosData
    {
        public double RA2000 { get; set; }
        public double Dec2000 { get; set; }
        public double PmRA { get; set; }
        public double PmDec { get; set; }
    }
}
