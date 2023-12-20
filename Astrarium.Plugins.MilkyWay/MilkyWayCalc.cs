using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Astrarium.Plugins.MilkyWay
{
    /// <summary>
    /// Calculates coordinates of Milky Way outline points to be rendered on map
    /// </summary>
    public class MilkyWayCalc : BaseCalc
    {
        /// <summary>
        /// Altitude of the Sun above horizon
        /// </summary>
        public double SunAltitude { get; private set; }

        public PrecessionalElements PrecessionElementsB1950ToCurrent { get; private set; }

        private readonly ISky sky;

        public MilkyWayCalc(ISky sky)
        {
            this.sky = sky;
        }

        public override void Calculate(SkyContext context)
        {
            // solar altitude, in degrees
            SunAltitude = context.Get(sky.SunEquatorial).ToHorizontal(context.GeoLocation, context.SiderealTime).Altitude;

            // precession elements from B1950 to current epoch
            PrecessionElementsB1950ToCurrent = Precession.ElementsFK5(Date.EPOCH_B1950, context.JulianDay);
        }
    }
}
