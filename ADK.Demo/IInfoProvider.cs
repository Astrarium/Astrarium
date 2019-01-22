using ADK.Demo.Objects;

namespace ADK.Demo
{
    public interface IInfoProvider<T> where T : CelestialObject
    {
        string GetInfo(SkyContext context, T body);
    }
}