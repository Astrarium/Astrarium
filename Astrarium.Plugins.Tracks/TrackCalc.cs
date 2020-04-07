using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Tracks
{
    /// <summary>
    /// Calculates coordinates of motion tracks of celestial bodies
    /// </summary>
    public class TrackCalc : BaseCalc
    {
        public ObservableCollection<Track> Tracks { get; } = new ObservableCollection<Track>();

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
