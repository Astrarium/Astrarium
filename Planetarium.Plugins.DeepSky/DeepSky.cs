using ADK;
using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.DeepSky
{
    public class DeepSky : SizeableCelestialObject
    {
        /// <summary>
        /// Number of record in the catalog file
        /// </summary>
        public short RecordNumber { get; set; }

        /// <summary>
        /// Flag indicating IC catalog
        /// </summary>
        public bool IC { get => RecordNumber >= 9106; }

        /// <summary>
        /// Catalog number
        /// </summary>
        public ushort Number { get; set; }

        public char Letter { get; set; }

        public char Component { get; set; }

        public byte Messier { get; set; }

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
        public float? Mag { get; set; }

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
        /// Proper name of NGC/IC object
        /// </summary>
        public string ProperName { get; set; }

        /// <summary>
        /// Gets name of deep sky object in NGC/IC catalog
        /// </summary>
        public string CatalogName { get => $"{(IC ? "IC" : "NGC")} {Number}{(Letter != ' ' ? new string(Letter, 1) : "")}{(Component != ' ' ? $"-{Component}" : "")}"; }

        /// <summary>
        /// Name of deep sky object to be displayed on map
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (ProperName != null)
                    return ProperName;
                else if (Messier > 0)
                    return $"M {(int)Messier}";
                else
                    return CatalogName;
            }
        }

        /// <summary>
        /// Gets array of all deep sky object names 
        /// </summary>
        public string[] AllNames
        {
            get
            {
                var names = new List<string>();
                if (ProperName != null)
                {
                    names.Add(ProperName);
                }
                if (Messier > 0)
                {
                    names.Add($"M {(int)Messier}");
                }
                names.Add(CatalogName);
                
                return names.ToArray();
            }
        }

        public override double Semidiameter { get => Math.Max(SizeA, SizeB) * 30; }

        public ICollection<CelestialPoint> Outline { get; set; }
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

    public static class DeepSkyStatusExtensions
    {
        public static bool IsEmpty(this DeepSkyStatus status)
        {
            return ((int)status > 5 || status == DeepSkyStatus.NotFound);
        }
    }
}
