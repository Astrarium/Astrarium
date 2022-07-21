using Astrarium.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Database.Entities
{
    public class ObservationDB : IEntity
    {
        public string Id { get; set; }
        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
        public string TargetId { get; set; }
        public string SessionId { get; set; }

        public double? Magnification { get; set; }
        public string Accessories { get; set; }

        public string Result { get; set; }

        /// <summary>
        /// Finding details specific for target type, in JSON form.
        /// </summary>
        public string Details { get; set; }

        public string ScopeId { get; set; }
        public string EyepieceId { get; set; }
        public string LensId { get; set; }
        public string FilterId { get; set; }
        public string ImagerId { get; set; }

        // related entities

        public virtual OpticsDB Scope { get; set; }
        public virtual EyepieceDB Eyepiece { get; set; }
        public virtual LensDB Lens { get; set; }
        public virtual FilterDB Filter { get; set; }
        public virtual TargetDB Target { get; set; }
        public virtual ImagerDB Imager { get; set; }
        public virtual ICollection<AttachmentDB> Attachments { get; set; }
    }
}
