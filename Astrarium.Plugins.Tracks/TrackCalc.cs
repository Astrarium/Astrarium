using Astrarium.Types;
using System.Collections.ObjectModel;

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
            
        }
    }
}
