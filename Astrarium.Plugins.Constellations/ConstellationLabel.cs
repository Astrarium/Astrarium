using Astrarium.Types;

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
        /// Cartesian coordinates of constellation label, for J2000.0
        /// </summary>
        public Vec3 Cartesian { get; set; }
    }
}
