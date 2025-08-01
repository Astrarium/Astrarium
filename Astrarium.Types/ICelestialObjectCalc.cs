using System;
using System.Collections.Generic;

namespace Astrarium.Types
{
    /// <summary>
    /// Defines methods for searching celestial objects
    /// </summary>
    public interface ICelestialObjectSearcher
    {
        /// <summary>
        /// Searches celestial objects by specified string
        /// </summary>
        /// <param name="context"><see cref="SkyContext"/> instance.</param>
        /// <param name="searchString">Search string.</param>
        /// <param name="maxCount">Maximal count of search results to return.</param>
        /// <param name="filterFunc">Filter function to match searching criteria.</param>
        /// <returns>Collection of celestial objects matching the search criteria.</returns>
        ICollection<CelestialObject> Search(SkyContext context, string searchString, Func<CelestialObject, bool> filterFunc, int maxCount = 50);

        /// <summary>
        /// Searches celestial object by exact matching of celestial object type and common name
        /// </summary>
        /// <param name="context"><see cref="SkyContext"/> instance.</param>
        /// <param name="bodyType">Celestial object type, for example, "Star" for stars.</param>
        /// <param name="bodyName">
        /// Common object name. Depending on object type it may be a catalog number (for example, for stars) 
        /// or English name (for example, for planets). 
        /// Common name should be a unique identifier of the celestial object 
        /// among other objects with same type.</param>
        /// <returns>Returns a single celestial object matching the type and common name, or null</returns>
        CelestialObject Search(SkyContext context, string bodyType, string bodyName);
    }

    /// <summary>
    /// Base interface for all modules which perform astronomical calculations for specific class of celestial bodies.
    /// </summary>
    /// <typeparam name="T">Type of celestial body</typeparam>
    public interface ICelestialObjectCalc<T> : ICelestialObjectSearcher where T : CelestialObject
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
        /// Enumerates celestial objects of given type which calculator operates of.
        /// The method can return empty enumerable if celestial objects should not be exposed outside the calculator.
        /// </summary>
        IEnumerable<T> GetCelestialObjects();
    }
}
