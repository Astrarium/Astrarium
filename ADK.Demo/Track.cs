using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo
{
    /// <summary>
    /// Describes motion track of celestial body
    /// </summary>
    public class Track
    {
        public CelestialObject Body { get; set; }

        public double FromJD { get; set; }

        public double ToJD { get; set; }

        /// <summary>
        /// Gets track coordinates calculations step, in days.
        /// </summary>
        public double Step
        {
            get
            {
                //double minStep = 1.0 / (24 * 60);

                //// average daily motion in degress
                double dailyMotion = (Body as IMovingObject).AverageDailyMotion;

                //double minuteMotion = dailyMotion / (24 * 60);

                return dailyMotion > 1 ? 1 / Math.Round(dailyMotion) : 1;
            }
        }

        /// <summary>
        /// Gets track duration, in days
        /// </summary>
        public double Duration => ToJD - FromJD;

        /// <summary>
        /// Track path points
        /// </summary>
        public IList<CelestialPoint> Points { get; } = new List<CelestialPoint>();

        public TimeSpan LabelsStep { get; set; }
    }
}
