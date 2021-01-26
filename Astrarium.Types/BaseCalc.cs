using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Astrarium.Types
{
    /// <summary>
    /// Base class for all modules which perform astronomical calculations.
    /// </summary>
    public abstract class BaseCalc : PropertyChangedBase
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
    /// Base interface for all modules which perform astronomical calculations for specific class of celestial bodies.
    /// </summary>
    /// <typeparam name="T">Type of celestial body</typeparam>
    public interface ICelestialObjectCalc<T> where T : CelestialObject
    {
        void ConfigureEphemeris(EphemerisConfig<T> config);
        void GetInfo(CelestialObjectInfo<T> info);
        ICollection<CelestialObject> Search(SkyContext context, string searchString, int maxCount = 50);
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
        public CancellationToken? CancelToken { get; set; }
    }
}
