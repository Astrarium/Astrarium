namespace Astrarium.Plugins.SolarSystem
{
    /// <summary>
    /// Descibes an active region that where observed on the previous solar 
    /// rotation and is due to reappear on the East limb in the 
    /// next 3 days.
    /// </summary>
    public class SolarRegionII : SolarRegion
    {
        /// <summary>
        /// Heliographic degrees latitude of the group on its last disk passage.
        /// </summary>
        public int Lat { get; set; }
    }
}
