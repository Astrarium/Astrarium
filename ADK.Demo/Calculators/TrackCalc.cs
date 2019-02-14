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
    public interface ITracksProvider
    {
        List<Track> Tracks { get; }
    }

    /// <summary>
    /// Calculates coordinates of motion tracks of celestial bodies
    /// </summary>
    public class TrackCalc : BaseCalc, ITracksProvider
    {
        public List<Track> Tracks { get; } = new List<Track>();

        public override void Calculate(SkyContext context)
        {
            foreach (var track in Tracks)
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
