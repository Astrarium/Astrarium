using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Planner
{
    public class PlanningFilter
    {
        public double JulianDayMidnight { get; set; }

        /// <summary>
        /// Begin of observation, in hours and fractions (time of the day)
        /// </summary>
        public double TimeFrom { get; set; }

        /// <summary>
        /// End of observation, in hours and fractions (time of the day)
        /// </summary>
        public double TimeTo { get; set; }

        /// <summary>
        /// Gets duration of observation, in hours and fractions
        /// </summary>
        public double Duration => TimeTo > TimeFrom ? TimeTo - TimeFrom : 24 - TimeFrom + TimeTo;

        public double? MaxSunAltitude { get; set; }
        public double? MinBodyAltitude { get; set; }
        public float? MagLimit { get; set; }
        public int? CountLimit { get; set; }
        public double? DurationLimit { get; set; }
        public bool SkipUnknownMagnitude { get; set; }

        public CrdsGeographical ObserverLocation { get; set; }
        public IReadOnlyCollection<string> ObjectTypes { get; set; }
    }
}
