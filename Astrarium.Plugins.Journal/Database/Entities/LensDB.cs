using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Database.Entities
{
    public class LensDB : IEntity
    {
        public string Id { get; set; }
        public string Model { get; set; }
        public string Vendor { get; set; }
        public double Factor { get; set; }
    }
}
