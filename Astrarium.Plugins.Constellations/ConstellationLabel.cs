using Astrarium.Algorithms;

namespace Astrarium.Plugins.Constellations
{
    public class ConstellationLabel
    {
        /// <summary>
        /// International abbreviated three-letter code of constellation.
        /// (+1 digit for "Ser1" and "Ser2" parts of Serpens constellation).
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Equatorial coordinates of constellation label, for J2000.0
        /// </summary>
        public CrdsEquatorial Equatorial0 { get; set; }
    }
}
