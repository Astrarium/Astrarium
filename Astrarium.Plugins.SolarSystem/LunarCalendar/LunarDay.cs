using Astrarium.Algorithms;
using Astrarium.Types;

namespace Astrarium.Plugins.SolarSystem
{
    public class LunarDay
    {
        public int DayOfMonth { get; set; }

        public Date Date { get; set; }
        /// <summary>
        /// JD of the midnight of current day
        /// </summary>
        public double JdMidnight { get; set; }

        /// <summary>
        /// Moon rise, transit and set for current day
        /// </summary>
        public RTS Moon { get; set; }

        /// <summary>
        /// Sun rise, transit and set for current day
        /// </summary>
        public RTS Sun { get; set; }

        /// <summary>
        /// Major phase, if occurs
        /// </summary>
        public MoonPhase? Phase { get; set; }

        /// <summary>
        /// Phase instant, if occurs
        /// </summary>
        public Date PhaseInstant { get; set; }

        /// <summary>
        /// Lunar phase, i.e. illumination
        /// </summary>
        public double Illumination { get; set; }

        /// <summary>
        /// Position angle of the Moon at culmination
        /// </summary>
        public double PositionAngle { get; set; }

        /// <summary>
        /// Lunar ephemerides
        /// </summary>
        public Ephemerides Ephemerides { get; set; }

        public double SiderealTime { get; set; }

        public CrdsEquatorial SunCoordinates { get; set; }

        public override string ToString()
        {
            return $"{DayOfMonth}{(Phase != null ? $"{Phase}" : "")}";
        }
    }
}
