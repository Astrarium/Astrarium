using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    public abstract class SolarSystemTargetDetails : TargetDetails { }

    [CelestialObjectType("Planet")]
    public class PlanetTargetDetails : SolarSystemTargetDetails { }

    [CelestialObjectType("Sun")]
    public class SunTargetDetails : SolarSystemTargetDetails { }

    [CelestialObjectType("Moon")]
    public class MoonTargetDetails : SolarSystemTargetDetails { }

    [CelestialObjectType("Asteroid")]
    public class AsteroidTargetDetails : SolarSystemTargetDetails { }

    [CelestialObjectType("Comet")]
    public class CometTargetDetails : SolarSystemTargetDetails { }
}
