using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Supernovae
{
    /// <summary>
    /// Represents supernova star
    /// </summary>
    public class Supernova : CelestialObject, IMagnitudeObject, IObservableObject
    {
        /// <inheritdoc />
        public override string Type => "Supernova";

        /// <inheritdoc />
        public override string CommonName => Name;

        /// <summary>
        /// Name of supernova star
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type of supernova star
        /// </summary>
        public string SupernovaType { get; set; }

        /// <summary>
        /// Visual magnitude at maximum
        /// </summary>
        public float MaxMagnitude { get; set; }

        /// <summary>
        /// Day of peak (maximum brightness)
        /// </summary>
        public double JulianDayPeak { get; set; }

        /// <summary>
        /// Current value of magnitude
        /// </summary>
        public float Magnitude { get; set; }

        /// <summary>
        /// Equatorial coordinates for J2000.0 epoch
        /// </summary>
        public CrdsEquatorial Equatorial0 { get; set; }

        /// <inheritdoc/>
        public override string[] Names => new[] { Name };

        /// <inheritdoc/>
        public override string[] DisplaySettingNames => new string[] { "Stars", "Supernovae" };
    }
}
