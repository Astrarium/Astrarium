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
        /// <summary>
        /// Creates new instance of TrackCalc
        /// </summary>
        /// <param name="sky"></param>
        public TrackCalc(Sky sky) : base(sky) { }

        public override void Calculate(SkyContext context)
        {
            var tracks = Sky.Get<ICollection<Track>>("Tracks");

            foreach (var track in tracks)
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
