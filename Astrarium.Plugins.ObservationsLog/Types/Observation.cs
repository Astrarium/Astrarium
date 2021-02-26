using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.ObservationsLog.Types
{
    public class Observation : IEntity
    {
        public string Id { get; set; }
        public DateTime Begin { get; set; }
        public DateTime? End { get; set; }
        public Target Target { get; set; }
        public string Result { get; set; }
    }
}
