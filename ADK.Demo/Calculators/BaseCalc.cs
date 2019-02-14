using ADK.Demo.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADK.Demo.Calculators
{
    public abstract class BaseCalc
    {
        public virtual void Initialize() { }
        public abstract void Calculate(SkyContext context);
    }

    public abstract class BaseCalc<T> : BaseCalc where T : CelestialObject
    {
        public virtual void ConfigureEphemeris(EphemerisConfig<T> config) { }
        public virtual CelestialObjectInfo GetInfo(SkyContext context, T body) { return null; }
        public virtual ICollection<SearchResultItem> Search(string searchString, int maxCount = 50) { return null; }
        public virtual ICollection<AstroEvent> GetEvents(ICelestialObjectsProvider celestialObjectsProvider, double jdFrom, double jdTo) { return null; }
    }
}
