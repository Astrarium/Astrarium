namespace Astrarium.Plugins.Journal.Types
{
    [CelestialObjectType("DeepSky.GalaxyCluster")]
    public class DeepSkyClusterOfGalaxiesTargetDetails : DeepSkyTargetDetails
    {
        /// <summary>
        /// Magnitude of the 10th brightest member in [mag] 
        /// </summary>
        public double? Mag10 { get; set; }
    }
}
