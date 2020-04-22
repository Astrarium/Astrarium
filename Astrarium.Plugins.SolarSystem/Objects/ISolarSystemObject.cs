namespace Astrarium.Plugins.SolarSystem.Objects
{
    public interface ISolarSystemObject
    {
        /// <summary>
        /// Distance from Earth, in AU
        /// </summary>
        double DistanceFromEarth { get; }
    }
}
