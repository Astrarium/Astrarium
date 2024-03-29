﻿using Astrarium.Algorithms;
using Astrarium.Types;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Astrarium.Plugins.Planner
{
    /// <summary>
    /// Holds filter conditions to build an observation plan.
    /// </summary>
    public class PlanningFilter
    {
        /// <summary>
        /// Gets or sets Julian Day for a local midnight of date of observation
        /// </summary>
        [JsonIgnore]
        public double JulianDayMidnight { get; set; }

        /// <summary>
        /// Geographical location of the observer
        /// </summary>
        [JsonIgnore]
        public CrdsGeographical ObserverLocation { get; set; }

        /// <summary>
        /// Begin of observation, in hours and fractions (time of the day)
        /// </summary>
        [JsonIgnore]
        public double TimeFrom { get; set; }

        /// <summary>
        /// End of observation, in hours and fractions (time of the day)
        [JsonIgnore]
        public double TimeTo { get; set; }

        /// <summary>
        /// Gets duration of observation, in hours and fractions
        /// </summary>
        [JsonIgnore]
        public double Duration => TimeTo > TimeFrom ? TimeTo - TimeFrom : 24 - TimeFrom + TimeTo;

        [JsonProperty("ApplyFilters")]
        public bool ApplyFilters { get; set; } = true;

        /// <summary>
        /// Maximal altitude of the Sun, in degrees, considering as observation condition.
        /// If not set (null), default altitude will be used, depending on the celestial object type.
        /// For example, planets considered to be observable with Sun altitude less than 0,
        /// deep sky objects are observable with Sun altitude less than -10.
        /// </summary>
        [JsonProperty("MaxSunAltitude")]
        public double? MaxSunAltitude { get; set; }

        /// <summary>
        /// Minimal altitude of celestial body, in degrees, considering as observation condition.
        /// If not set (null), default altitude will be used, depending on the celestial object type.
        /// Usual it's 5 degrees.
        /// </summary>
        [JsonProperty("MinBodyAltitude")]
        public double? MinBodyAltitude { get; set; } = 5;

        /// <summary>
        /// Minimal magnitude of the celestial body, to include it into observation plan.
        /// If not set (null), there are no limitations.
        /// </summary>
        [JsonProperty("MagLimit")]
        public float? MagLimit { get; set; }

        /// <summary>
        /// Count of celestial objects to include it into observation plan.
        /// If not set (null), there are no limitations.
        /// </summary>
        [JsonProperty("CountLimit")]
        public int? CountLimit { get; set; } = 1000;

        /// <summary>
        /// Minimal duration of the observation, in minutes.
        /// If a celestial body is observable during lesser period, it's neglected.
        /// </summary>
        [JsonProperty("DurationLimit")]
        public double? DurationLimit { get; set; } = 10;

        /// <summary>
        /// Flag indicating objects with unknown magnitude should be discarded.
        /// </summary>
        [JsonProperty("SkipUnknownMagnitude")]
        public bool SkipUnknownMagnitude { get; set; }

        /// <summary>
        /// Collection of celestial object types (string keys) to be observed.
        /// </summary>
        [JsonProperty("CelestialObjectsTypes")]
        public IReadOnlyCollection<string> CelestialObjectsTypes { get; set; } = new string[0];

        /// <summary>
        /// Collection of celestial objects to create plan from.
        /// </summary>
        [JsonIgnore]
        public IReadOnlyCollection<CelestialObject> CelestialObjects { get; set; }
    }
}
