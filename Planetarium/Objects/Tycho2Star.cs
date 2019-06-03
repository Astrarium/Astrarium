using ADK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Objects
{
    public class Tycho2Star :  CelestialObject
    {
        public CrdsEquatorial Equatorial0 { get; set; }
        public short Tyc1 { get; set; }
        public short Tyc2 { get; set; }
        public char Tyc3 { get; set; }
        public float Magnitude { get; set; }

        public override string ToString()
        {
            return string.Format("TYC {0}-{1}-{2}", Tyc1, Tyc2, Tyc3);
        }
    }
}
