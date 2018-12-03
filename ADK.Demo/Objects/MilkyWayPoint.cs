namespace ADK.Demo.Objects
{
    public class MilkyWayPoint
    {        
        /// <summary>
        /// Equatorial coordinates of a point referred to J2000.0 epoch
        /// </summary>
        public CrdsEquatorial Equatorial0 { get; set; } = new CrdsEquatorial();

        /// <summary>
        /// Local horizontal coordinates
        /// </summary>
        public CrdsHorizontal Horizontal { get; set; }
    }
}
