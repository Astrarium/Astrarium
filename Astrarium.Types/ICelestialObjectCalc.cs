using System.Collections.Generic;

namespace Astrarium.Types
{
    /// <summary>
    /// Base interface for all modules which perform astronomical calculations for specific class of celestial bodies.
    /// </summary>
    /// <typeparam name="T">Type of celestial body</typeparam>
    public interface ICelestialObjectCalc<T> where T : CelestialObject
    {
        /// <summary>
        /// Defines available ephemerides for celestial object of given type.
        /// </summary>
        /// <param name="definition">Ephemerides definition</param>
        void ConfigureEphemeris(EphemerisConfig<T> definition);

        /// <summary>
        /// Gets information about
        /// </summary>
        /// <param name="info"></param>
        void GetInfo(CelestialObjectInfo<T> info);
        
        /// <summary>
        /// Searches celestial objects by specified string
        /// </summary>
        /// <param name="context"><see cref="SkyContext"/> instance.</param>
        /// <param name="searchString">Search string.</param>
        /// <param name="maxCount">Maximal count of search results to return.</param>
        /// <returns>Collection of celestial objects matching the search criteria.</returns>
        ICollection<CelestialObject> Search(SkyContext context, string searchString, int maxCount = 50);
        
        /// <summary>
        /// Enumerates celestial objects of given type which calculator operates of.
        /// The method can return empty enumerable if celestial objects should not be exposed outside the calculator.
        /// </summary>
        IEnumerable<T> GetCelestialObjects();
    }
}
