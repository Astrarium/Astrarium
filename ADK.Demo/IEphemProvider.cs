using ADK.Demo.Objects;

namespace ADK.Demo
{
    public interface IEphemProvider<T> where T : CelestialObject
    {
        void ConfigureEphemeris(EphemerisConfig<T> config);
    }
}