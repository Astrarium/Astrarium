using Astrarium.Algorithms;
using Astrarium.Types;
using System;

namespace Astrarium.Plugins.Grids
{
    public class CelestialGridCalculator : BaseCalc
    {
        /// <summary>
        /// Longitude of lunar ascending node, in radians
        /// </summary>
        public double LunarAscendingNodeLongitude { get; private set; }

        /// <summary>
        /// Used for rendering of Galactical equator
        /// </summary>
        public PrecessionalElements PrecessionalElementsB1950ToCurrent { get; private set; }

        public override void Calculate(SkyContext context)
        {
            PrecessionalElementsB1950ToCurrent = Precession.ElementsFK5(Date.EPOCH_B1950, context.JulianDay);
            LunarAscendingNodeLongitude = LunarEphem.TrueAscendingNode(context.JulianDay);
        }
    }
}
