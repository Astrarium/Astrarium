using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.ObservationsLog.Types
{
    public class Site : IEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double? Elevation { get; set; }
        public double TimeZone { get; set; }
        public string IAUCode { get; set; }
    }
}
