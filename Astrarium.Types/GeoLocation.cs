using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Types
{
    public class GeoLocation : CrdsGeographical
    {
        public string[] Names { get; set; }
        public string Country { get; set; }

        public double DistanceTo(CrdsGeographical g)
        {
            return g.DistanceTo(new CrdsGeographical(Longitude, Latitude));
        }
    }
}
