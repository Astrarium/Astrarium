using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Calculators
{
    /// <summary>
    /// Calculates coordinates of motion tracks of celestial bodies
    /// </summary>
    public class TrackCalc : BaseSkyCalc
    {
        private ITracksProvider TracksProvider;

        /// <summary>
        /// Creates new instance of TrackCalc
        /// </summary>
        public TrackCalc(ITracksProvider tracksProvider)
        {
            TracksProvider = tracksProvider;
        }

        public override void Calculate(SkyContext context)
        {
            foreach (var track in TracksProvider.Tracks)
            {
                foreach (var tp in track.Points)
                {
                    // Apparent horizontal coordinates
                    tp.Horizontal = tp.Equatorial0.ToHorizontal(context.GeoLocation, context.SiderealTime);
                }
            }
        }
    }
}
