namespace Astrarium.Plugins.SolarSystem.Objects
{
    /// <summary>
    /// Interface for all objects inside Solar System which have measurable distance from the Earth
    /// </summary>
    public interface ISolarSystemObject
    {
        /// <summary>
        /// Distance from the Earth, in AU
        /// </summary>
        double DistanceFromEarth { get; }
    }
}
