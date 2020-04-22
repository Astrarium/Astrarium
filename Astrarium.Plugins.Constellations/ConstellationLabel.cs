using Astrarium.Types;

namespace Astrarium.Plugins.Constellations
{
    public class ConstellationLabel : CelestialPoint
    {
        /// <summary>
        /// International abbreviated three-letter code of constellation.
        /// (+1 digit for "Ser1" and "Ser2" parts of Serpens constellation).
        /// </summary>
        public string Code { get; set; }
    }
}
