using Astrarium.Types;

namespace Astrarium.Plugins.Journal.Types
{
    public class TargetDetails
    {
        [Ephemeris("Equatorial.Alpha")]
        public double? RA { get; set; }

        [Ephemeris("Equatorial.Delta")]
        public double? Dec { get; set; }

        [Ephemeris("Constellation")]
        public string Constellation { get; set; }
    }
}
