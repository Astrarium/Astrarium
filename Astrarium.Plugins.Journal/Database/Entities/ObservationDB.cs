using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObservationPlannerDatabase.Database.Entities
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
        /// Finding details are specific for target type
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

    public class DeepSkyObservationDetails
    {
        public double? SmallDiameter { get; set; }
        public double? LargeDiameter { get; set; }
        public bool? Stellar { get; set; }
        public bool? Extended { get; set; }
        public bool? Resolved { get; set; }
        public bool? Mottled { get; set; }

        /// <summary>
        /// Rating according to the scale of the "Deep Sky Liste", 99 means "unknown"
        /// </summary>
        public int Rating { get; set; }
    }

    public class DoubleStarObservationDetails : DeepSkyObservationDetails
    {
        /// <summary>
        /// Color of main component, string with possible values:
        /// "white", "red", "orange", "yellow", "green", "blue"
        /// </summary>
        public string ColorMainComponent { get; set; }

        /// <summary>
        /// Color of main component, string with possible values:
        /// "white", "red", "orange", "yellow", "green", "blue"
        /// </summary>
        public string ColorCompainionComponent { get; set; }

        public bool? EqualBrightness { get; set; }
        public bool? NiceSurrounding { get; set; }
    }

    public class OpenClusterObservationDetails : DeepSkyObservationDetails
    {
        /// <summary>
        /// Character of the cluster according to "Deep Sky Liste" definition
        /// </summary>
        public string Character { get; set; }

        public bool? UnusualShape { get; set; }
        public bool? PartlyUnresolved { get; set; }
        public bool? ColorContrasts { get; set; }
    }

    public class VariableStarObservationDetails
    {
        public string ChartDate { get; set; }
        public bool? NonAAVSOChart { get; set; }
        public string ComparisonStars { get; set; }

        public bool? BrightSky { get; set; }
        public bool? Clouds { get; set; }
        public bool? PoorSeeing { get; set; }
        public bool? NearHorizion { get; set; }
        public bool? UnusualActivity { get; set; }
        public bool? Outburst { get; set; }
        public bool? ComparismSequenceProblem { get; set; }
        public bool? StarIdentificationUncertain { get; set; }
        public bool? FaintStar { get; set; }
    }
}
