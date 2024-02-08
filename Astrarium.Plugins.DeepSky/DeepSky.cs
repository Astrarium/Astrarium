using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;

namespace Astrarium.Plugins.DeepSky
{
    public class DeepSky : SizeableCelestialObject, IMagnitudeObject, IObservableObject
    {
        /// <inheritdoc />
        public override string Type => Status.IsEmpty() ? null : $"DeepSky.{Status}";

        /// <inheritdoc />
        public override string CommonName => CatalogName;

        /// <summary>
        /// Number of record in the catalog file
        /// </summary>
        public short RecordNumber { get; set; }

        /// <summary>
        /// Flag indicating IC catalog
        /// </summary>
        public bool IC { get => RecordNumber >= 9106; }

        /// <summary>
        /// NGC/IC Catalog number
        /// </summary>
        public ushort Number { get; set; }

        /// <summary>
        /// NGC/IC component letter, if applicable
        /// </summary>
        public char Letter { get; set; }

        /// <summary>
        /// NGC/IC component name, if applicable (A, B, etc.)
        /// </summary>
        public char Component { get; set; }

        /// <summary>
        /// Messier catalog number
        /// </summary>
        public byte Messier { get; set; }

        /// <summary>
        /// Equatorial coordinates for epoch J2000.0
        /// </summary>
        public CrdsEquatorial Equatorial0 { get; set; }

        /// <summary>
        /// Status of deep sky object
        /// </summary>
        public DeepSkyStatus Status { get; set; }

        /// <summary>
        /// Visual (if present) or photographic magnitude
        /// </summary>
        public float Magnitude { get; set; }

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
        public override string[] Names
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

        /// <inheritdoc />
        public override float Semidiameter => Math.Max(LargeSemidiameter ?? 0, SmallSemidiameter ?? 0);

        /// <inheritdoc />
        public override double? ShapeEpoch => Date.EPOCH_J2000;

        /// <summary>
        /// Name of the setting(s) responsible for displaying the object
        /// </summary>
        public override string[] DisplaySettingNames => new[] { "DeepSky" };
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
