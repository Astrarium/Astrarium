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
        public List<Observation> Observations { get; set; } = new List<Observation>();
    }
}
