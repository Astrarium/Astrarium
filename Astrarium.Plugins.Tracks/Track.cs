﻿using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Astrarium.Plugins.Tracks
{
    /// <summary>
    /// Describes motion track of celestial body
    /// </summary>
    public class Track
    {
        /// <summary>
        /// Unique track identifier, used internally
        /// </summary>
        public Guid Id { get; set; }

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
                // mean daily motion, in degrees
                double dailyMotion = (Body as IMovingObject).AverageDailyMotion;

                // recommended calculation step, in days
                double calcStep = dailyMotion > 1 ? 1 / dailyMotion : 1;

                // labels step, in days
                double labelsStep = LabelsStep.TotalDays;

                return DrawLabels ? Math.Min(calcStep, labelsStep) : calcStep;
            }
        }

        /// <summary>
        /// Gets track duration, in days
        /// </summary>
        public double Duration => To - From;

        /// <summary>
        /// Track path points
        /// </summary>
        public IList<CrdsEquatorial> Points { get; } = new List<CrdsEquatorial>();

        /// <summary>
        /// Desired labels step. This value should not be less than calculation step.
        /// To check this, compare desired value with <see cref="SmallestLabelsStep(IMovingObject)"/> result.
        /// </summary>
        public TimeSpan LabelsStep { get; set; }

        /// <summary>
        /// Track color
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Flag indicating is it required to draw track labels or not
        /// </summary>
        public bool DrawLabels { get; set; }

        /// <summary>
        /// Gets smallest allowed labels step for the celestial body
        /// </summary>
        /// <param name="body">Celestial body to get smallest labels step. Should implement interface <see cref="IMovingObject"/>.</param>
        /// <returns></returns>
        public double SmallestLabelsStep()
        {
            // mean daily motion, in degrees
            double dailyMotion = (Body as IMovingObject).AverageDailyMotion;

            // recommended calculation step, in days
            return dailyMotion > 1 ? 1 / dailyMotion : dailyMotion;
        }
    }
}
