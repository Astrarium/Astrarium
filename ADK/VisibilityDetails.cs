using System;

namespace ADK
{
    /// <summary>
    /// Describes conditions of visibility of celestial body  
    /// </summary>
    public class VisibilityDetails
    {
        /// <summary>
        /// Visibility duration, in hours
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// Visibility period (period of day when the body is observable) 
        /// </summary>
        public VisibilityPeriod Period { get; set; }

        /// <summary>
        /// Begin of visibility, in fractions of day
        /// </summary>
        public double Begin { get; set; }

        /// <summary>
        /// End of visibility, in fractions of day
        /// </summary>
        public double End { get; set; }
    }

    /// <summary>
    /// Defines periods of day when a celestial body is observable
    /// </summary>
    [Flags]
    public enum VisibilityPeriod
    {
        /// <summary>
        /// Body is invisible during the day
        /// </summary>
        Invisible = 0,

        /// <summary>
        /// Body can be observed in the evening
        /// </summary>
        Evening = 1,

        /// <summary>
        /// Body can be observed in the night (i.e. after midnight) 
        /// </summary>
        Night = 2,

        /// <summary>
        /// Body can be observed in the morning (before sunrise)
        /// </summary>
        Morning = 4,

        /// <summary>
        /// Body can be observed the whole night
        /// </summary>
        WholeNight = Evening | Night | Morning
    }
}