using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Objects
{
    public class DeepSky : CelestialObject
    {
        /// <summary>
        /// Equatorial coordinates for epoch J2000.0
        /// </summary>
        public CrdsEquatorial Equatorial0 { get; set; }

        /// <summary>
        /// Equatorial coordinates for current epoch
        /// </summary>
        public CrdsEquatorial Equatorial { get; set; }

        /// <summary>
        /// Status of deep sky object
        /// </summary>
        public DeepSkyStatus Status { get; set; }

        /// <summary>
        /// Visual (if present) or photographic magnitude
        /// </summary>
        public float Mag { get; set; }

        /// <summary>
        /// Larger diameter, in seconds of arc
        /// </summary>
        public float SizeA { get; set; }

        /// <summary>
        /// Smaller diameter, in seconds of arc
        /// </summary>
        public float SizeB { get; set; }

        /// <summary>
        /// Position angle
        /// </summary>
        public short PA { get; set; }

        /// <summary>
        /// Deep sky type
        /// </summary>
        public string Type { get; set; }
    }

    public enum DeepSkyStatus : byte
    {
        Galaxy          = 1,
        GalacticNebula  = 2,
        PlanetaryNebula = 3,
        OpenCluster     = 4,
        GlobularCluster = 5,
        PartOfOther     = 6,
        Duplicate       = 7,
        DuplicateIC     = 8,
        Star            = 9,
        NotFound        = 0
    }
}
