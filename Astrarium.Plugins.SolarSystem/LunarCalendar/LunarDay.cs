using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.SolarSystem
{
    public class LunarDay
    {
        public int DayOfMonth { get; set; }

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
        /// Localized phase text, like New Moon ot Full Moon
        /// </summary>
        public string PhaseText { get; set; }

        /// <summary>
        /// Phase instant, if occurs
        /// </summary>
        public double PhaseInstant { get; set; }

        public string PhaseInstantString { get; set; }

        /// <summary>
        /// Lunar phase, i.e. illumination
        /// </summary>
        public double Illumination { get; set; }

        /// <summary>
        /// Position angle of the Moon at culmination
        /// </summary>
        public double PositionAngle { get; set; }

        public override string ToString()
        {
            return $"{DayOfMonth}{(Phase != null ? $"{Phase}" : "")}";
        }
    }
}
