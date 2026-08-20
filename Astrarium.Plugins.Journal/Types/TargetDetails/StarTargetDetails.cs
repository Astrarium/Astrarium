using Astrarium.Types;

namespace Astrarium.Plugins.Journal.Types
{
    [CelestialObjectType("Star")]
    public class StarTargetDetails : TargetDetails
    {
        [Ephemeris("Magnitude")]
        public double? Magnitude { get; set; }

        [Ephemeris("SpectralClass")]
        public string Classification { get; set; }
    }
}
