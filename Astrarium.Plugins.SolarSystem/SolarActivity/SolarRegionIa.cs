using Astrarium.Algorithms;

namespace Astrarium.Plugins.SolarSystem
{
    /// <summary>
    /// Describes previously numbered active regions which still 
    /// contain plage but no visible sunspots.
    /// </summary>
    public class SolarRegionIa
    {
        /// <summary>
        /// SESC region number.
        /// </summary>
        public int Nmbr { get; set; }

        /// <summary>
        /// Plage region location in heliographic degrees latitude and 
        /// degrees east or west from central meridian rotated to 2400 UTC.
        /// </summary>
        public CrdsHeliographical Location { get; private set; } = new CrdsHeliographical();

        /// <summary>
        /// Carrington longitude of the region.
        /// </summary>
        public int Lo { get; set; }
    }
}
