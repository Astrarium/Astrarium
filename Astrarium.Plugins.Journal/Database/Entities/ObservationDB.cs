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
        /// <inheritdoc />
        public string Id { get; set; }

        /// <summary>
        /// Begin date and time of observation
        /// </summary>
        public string Begin { get; set; }

        /// <summary>
        /// End date and time of observation
        /// </summary>
        public string End { get; set; }

        /// <summary>
        /// Id of parent session
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Magnification used (optional)
        /// </summary>
        public double? Magnification { get; set; }

        /// <summary>
        /// Accessories (free text)
        /// </summary>
        public string Accessories { get; set; }

        /// <summary>
        /// Observation result - result of your findings
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// Language (2-letter ISO code)
        /// </summary>
        public string Lang { get; set; }

        /// <summary>
        /// Finding details specific for target type, in JSON form.
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// Id of target (celestial object)
        /// </summary>
        public string TargetId { get; set; }

        /// <summary>
        /// Id of scope/optics used
        /// </summary>
        public string ScopeId { get; set; }

        /// <summary>
        /// Id of eyepiece used
        /// </summary>
        public string EyepieceId { get; set; }

        /// <summary>
        /// Id of lens used
        /// </summary>
        public string LensId { get; set; }

        /// <summary>
        /// Id of filter used
        /// </summary>
        public string FilterId { get; set; }

        /// <summary>
        /// Id of imager (camera/CCD) used
        /// </summary>
        public string CameraId { get; set; }

        // related entities

        public virtual OpticsDB Scope { get; set; }
        public virtual EyepieceDB Eyepiece { get; set; }
        public virtual LensDB Lens { get; set; }
        public virtual FilterDB Filter { get; set; }
        public virtual TargetDB Target { get; set; }
        public virtual CameraDB Camera { get; set; }
        public virtual ICollection<AttachmentDB> Attachments { get; set; }
    }
}
