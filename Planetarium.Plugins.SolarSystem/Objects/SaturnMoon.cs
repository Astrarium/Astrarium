﻿using ADK;
using Planetarium.Types.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Objects
{
    /// <summary>
    /// Contains coordinates and visual appearance data for the major moon of Saturn for given instant of time.
    /// </summary>
    public class SaturnMoon : SizeableCelestialObject, IPlanetMoon, ISolarSystemObject
    {
        public SaturnMoon(int number)
        {
            Number = number;
        }

        /// <summary>
        /// Apparent equatorial coordinates of the Saturn moon
        /// </summary>
        public CrdsEquatorial Equatorial { get; internal set; }

        /// <summary>
        /// Planetocentric rectangular coordinates of the Saturn moon
        /// </summary>
        public CrdsRectangular Rectangular { get; internal set; }

        /// <summary>
        /// Name of the Saturn moon
        /// </summary>
        public string Name => Text.Get($"SaturnMoon.{Number}.Name");

        /// <summary>
        /// Gets Saturn moon names
        /// </summary>
        public override string[] Names => new[] { Name };

        public double DistanceFromEarth { get; internal set; }

        /// <summary>
        /// Number of the moon
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// Longitude of central meridian
        /// </summary>
        public double CM { get; internal set; }

        /// <summary>
        /// Apparent magnitude
        /// </summary>
        public float Magnitude { get; internal set; }

        public bool IsEclipsedByPlanet => false;
    }
}