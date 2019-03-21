using ADK;
using Planetarium.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Calculators
{
    /// <summary>
    /// Base class for all modules which perform astronomical calculations.
    /// </summary>
    public abstract class BaseCalc
    {
        /// <summary>
        /// Performs starting initialization of the calculator.
        /// It's a good place to load data required by the module.
        /// Base implementation does nothing, you could skip this method if no initialization is required.
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// This method is called when it's needed to perform astronomical calculations before rendering the sky map.
        /// </summary>
        /// <param name="context"></param>
        public abstract void Calculate(SkyContext context);
    }

    /// <summary>
    /// Base class for all modules which perform astronomical calculations for specific class of celestial bodies.
    /// </summary>
    /// <typeparam name="T">Type of celestial body</typeparam>
    public abstract class BaseCalc<T> : BaseCalc where T : CelestialObject
    {
        public virtual void ConfigureEphemeris(EphemerisConfig<T> config) { }
        public virtual CelestialObjectInfo GetInfo(SkyContext context, T body) { return null; }
        public virtual ICollection<SearchResultItem> Search(string searchString, int maxCount = 50) { return null; }
        public virtual string GetName(T body) { return body.ToString(); }
    }

    public abstract class BaseAstroEventsProvider
    {
        public abstract void ConfigureAstroEvents(AstroEventsConfig config);
    }

    public class AstroEventsContext : Memoizer<AstroEventsContext>
    {
        public CrdsGeographical GeoLocation { get; set; }
        public double From { get; set; }
        public double To { get; set; }
    }
}
