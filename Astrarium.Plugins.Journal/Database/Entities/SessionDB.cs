using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObservationPlannerDatabase.Database.Entities
{
    public class SessionDB : IEntity
    {
        public string Id { get; set; }
        public DateTime Begin { get; set; }
        public DateTime End { get; set; }

        public int? Seeing { get; set; }
        public double? SkyQuality { get; set; }
        public double? FaintestStar { get; set; }
        public string Weather { get; set; }
        public string Equipment { get; set; }
        public string Comments { get; set; }

        public string SiteId { get; set; }
        public string ObserverId { get; set; }

        public virtual SiteDB Site { get; set; }
        public virtual ObserverDB Observer { get; set; }
        public virtual ICollection<ObserverDB> CoObservers { get; set; }
        public virtual ICollection<ObservationDB> Observations { get; set; }
        public virtual ICollection<AttachmentDB> Attachments { get; set; }
    }
}
