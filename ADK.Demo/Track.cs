﻿using ADK.Demo.Objects;
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
        /// <summary>
        /// Celestial body the track belongs to
        /// </summary>
        public CelestialObject Body { get; set; }

        /// <summary>
        /// Starting Julian Day
        /// </summary>
        public double From { get; set; }

        /// <summary>
        /// Ending Julian Day
        /// </summary>
        public double To { get; set; }

        /// <summary>
        /// Gets track coordinates calculations step, in days.
        /// </summary>
        public double Step
        {
            get
            {
                // smallest labels step = default calculation step
                double step = SmallestLabelsStep(Body as IMovingObject);

                // labels step, in days
                double labelsStep = LabelsStep.TotalDays;
                
                if (step <= labelsStep)
                {
                    int f = (int)Math.Round(labelsStep / step);
                    return labelsStep / f;
                }
                else
                {
                    return step;
                }
            }
        }

        /// <summary>
        /// Gets track duration, in days
        /// </summary>
        public double Duration => To - From;

        /// <summary>
        /// Track path points
        /// </summary>
        public IList<CelestialPoint> Points { get; } = new List<CelestialPoint>();

        /// <summary>
        /// Desired labels step. This value should not be less than calculation step.
        /// To check this, compare desired value with <see cref="SmallestLabelsStep(IMovingObject)"/> result.
        /// </summary>
        public TimeSpan LabelsStep { get; set; }

        public static double SmallestLabelsStep(IMovingObject body)
        {
            // mean daily motion, in degrees
            double dailyMotion = (body as IMovingObject).AverageDailyMotion;

            // recommended calculation step, in days
            return dailyMotion > 1 ? 1 / dailyMotion : 1;
        }
    }
}