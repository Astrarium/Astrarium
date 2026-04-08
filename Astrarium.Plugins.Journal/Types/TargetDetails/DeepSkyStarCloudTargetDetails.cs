namespace Astrarium.Plugins.Journal.Types
{
    [CelestialObjectType("DeepSky.StarCloud")]
    public class DeepSkyStarCloudTargetDetails : DeepSkyTargetDetails
    {
        /// <summary>
        /// Position angle of axis, in degrees
        /// </summary>
        [Ephemeris("PositionAngle")]
        public int? PositionAngle { get; set; }
    }
}
