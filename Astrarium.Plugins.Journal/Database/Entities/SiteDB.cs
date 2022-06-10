using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObservationPlannerDatabase.Database.Entities
{
    public class SiteDB : IEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Elevation { get; set; }
        public double Timezone { get; set; }
        public string IAUCode { get; set; }
    }
}
