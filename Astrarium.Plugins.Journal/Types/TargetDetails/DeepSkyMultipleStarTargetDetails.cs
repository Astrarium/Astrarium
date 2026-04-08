namespace Astrarium.Plugins.Journal.Types
{
    [CelestialObjectType("DeepSky.MultipleStar")]
    public class DeepSkyMultipleStarTargetDetails : DeepSkyTargetDetails
    {
        /// <summary>
        /// Star which are components of the multiple star system. Must occur at least three times. 
        /// </summary>
        public string[] Component { get; set; }
    }
}
