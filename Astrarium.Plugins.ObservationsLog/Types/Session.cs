using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.ObservationsLog.Types
{
    public class Session : IEntity
    {
        public string Id { get; set; }


        public string Seeing { get; set; }
        public double? FaintestStar { get; set; }
        public double? SkyBrightness { get; set; }

        public string Weather { get; set; }
        public string Comments { get; set; }

        public Site Site { get; set; }
        public Observer Observer { get; set; }
        public List<Observation> Observations { get; set; } = new List<Observation>();
    }
}
