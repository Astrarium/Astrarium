using Astrarium.Types;

namespace Astrarium.Plugins.Journal.Types
{
    public interface ITargetDetailsFactory
    {
        TargetDetails BuildTargetDetails(CelestialObject body, SkyContext context);
    }
}