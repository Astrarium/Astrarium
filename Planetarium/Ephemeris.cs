using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium
{
    public class Ephemeris
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public IEphemFormatter Formatter { get; set; }
    }
}
