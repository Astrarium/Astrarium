using Astrarium.Algorithms;

namespace Astrarium.Plugins.SolarSystem
{
    public abstract class ActiveSolarRegion : SolarRegion
    {
        /// <summary>
        /// Region location, in heliographic degrees latitude and 
        /// degrees east or west from central meridian, rotated to 2400 UTC.
        /// </summary>
        public CrdsHeliographical Location { get; private set; } = new CrdsHeliographical();
    }
}
