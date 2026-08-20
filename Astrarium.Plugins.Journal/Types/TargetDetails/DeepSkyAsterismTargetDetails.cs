namespace Astrarium.Plugins.Journal.Types
{
    [CelestialObjectType("DeepSky.Asterism")]
    public class DeepSkyAsterismTargetDetails : DeepSkyTargetDetails
    {
        /// <summary>
        /// Position angle of axis, in degrees
        /// </summary>
        [Ephemeris("PositionAngle")]
        public int? PositionAngle { get; set; }
    }
}
