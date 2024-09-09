using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Astrarium.Types
{
    public interface ISky
    {
        SkyContext Context { get; }
        event Action Calculated;
        event Action<bool> TimeSyncChanged;

        void SetDate(double jd);
        void Calculate();
        bool TimeSync { get; set; }

        /// <summary>
        /// Gets ephemerides of specified celestial object
        /// </summary>
        ICollection<Ephemerides> GetEphemerides(CelestialObject body, double from, double to, double step, IEnumerable<string> categories, CancellationToken? cancelToken = null, IProgress<double> progress = null);
        Ephemerides GetEphemerides(CelestialObject body, SkyContext context, IEnumerable<string> categories);
        ICollection<string> GetEphemerisCategories(CelestialObject body);
        ICollection<AstroEvent> GetEvents(double jdFrom, double jdTo, IEnumerable<string> categories, CancellationToken? cancelToken = null);
        ICollection<string> GetEventsCategories();
        CelestialObjectInfo GetInfo(CelestialObject body);
        Constellation GetConstellation(string code);

        // TODO: move to ISkyMap
        ICollection<Tuple<int, int>> ConstellationLines { get; set; }
        IDictionary<string, string> StarNames { get; }
        IEnumerable<CelestialObject> CelestialObjects { get; }

        /// <summary>
        /// Adds cross-reference data for celestial objects with specified type.
        /// This should be used generally for adding cross-references between star catalogues to exclude duplicate entries 
        /// on the sky map and in search results.
        /// </summary>
        /// <param name="celestialObjectType">Unique celestial object type code.</param>
        /// <param name="crossReferences">Dictionary of cross identifiers. Key is a primary catalog name of the object, 
        /// for example, star name in BSC (Bright Star Catalog), value is a secondary (large) catalog name, for example, star name in Tycho2 catalogue.
        /// </param>
        void AddCrossReferences(string celestialObjectType, IDictionary<string, string> crossReferences);
        
        /// <summary>
        /// Gets cross-reference (supplementary) names of the celestial body in extended catalogs.
        /// </summary>
        /// <param name="body">Celestial body to get supplementary cross-reference identifiers.</param>
        /// <returns>Collection of celestial object names in other catalogs.</returns>
        ICollection<string> GetCrossReferences(CelestialObject body);
        
        /// <summary>
        /// Function that gets equatorial coordinates of the Sun
        /// </summary>
        Func<SkyContext, CrdsEquatorial> SunEquatorial { get; set; }

        /// <summary>
        /// Function that gets equatorial coordinates of the Moon
        /// </summary>
        Func<SkyContext, CrdsEquatorial> MoonEquatorial { get; set; }

        /// <summary>
        /// Searches celestial objects by string.
        /// </summary>
        /// <param name="searchString">String containing object name.</param>
        /// <param name="filter">Function to filter search results.</param>
        /// <param name="maxCount">Maximal number of objects to be returned.</param>
        /// <returns></returns>
        ICollection<CelestialObject> Search(string searchString, Func<CelestialObject, bool> filter, int maxCount = 50);

        /// <summary>
        /// Searches celestial object by its type and name. 
        /// </summary>
        /// <param name="objectType">Unique string identifier of celestial object type, see <see cref="CelestialObject.Type"/></param>
        /// <param name="objectName">Unique common name of the celestial object, see <see cref="CelestialObject.CommonName"/>. Can be omitted in case of single objects (like Sun or Moon).</param>
        /// <returns></returns>
        CelestialObject Search(string objectType, string commonName = null);
    }
}