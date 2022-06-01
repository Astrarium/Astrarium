using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Novae
{
    /// <summary>
    /// Represents nova star
    /// </summary>
    public class Nova : CelestialObject
    {
        /// <inheritdoc />
        public override string Type => "Nova";

        /// <inheritdoc />
        public override string CommonName => Name;

        /// <summary>
        /// Variable name of nova star
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Proper name of nova star, like "Nova Cassiopeiae 2021"
        /// </summary>
        public string ProperName { get; set; }

        /// <summary>
        /// Type of nova star
        /// </summary>
        public string NovaType { get; set; }

        /// <summary>
        /// Visual magnitude at maximum
        /// </summary>
        public float MaxMagnitude { get; set; }

        /// <summary>
        /// Visual magnitude at minimum
        /// </summary>
        public float MinMagnitude { get; set; }

        /// <summary>
        /// Day of peak (maximum brightness)
        /// </summary>
        public double JulianDayPeak { get; set; }

        /// <summary>
        /// Current value of magnitude
        /// </summary>
        public float Mag { get; set; }

        /// <summary>
        /// Equatorial coordinates for J2000.0 epoch
        /// </summary>
        public CrdsEquatorial Equatorial0 { get; set; }

        /// <summary>
        /// Number of days from peak to decline brightness of 2 magnitudes
        /// </summary>
        public int? M2 { get; set; }

        /// <summary>
        /// Number of days from peak to decline brightness of 3 magnitudes
        /// </summary>
        public int? M3 { get; set; }

        /// <summary>
        /// Number of days from peak to decline brightness of 6 magnitudes
        /// </summary>
        public int? M6 { get; set; }

        /// <summary>
        /// Number of days from peak to decline brightness of 9 magnitudes
        /// </summary>
        public int? M9 { get; set; }

        /// <inheritdoc/>
        public override string[] Names => new[] { ProperName, Name };

        /// <inheritdoc/>
        public override string[] DisplaySettingNames => new string[] { "Stars", "Novae" };
    }
}
