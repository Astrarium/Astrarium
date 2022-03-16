using System;
using System.Collections.Generic;
using System.Threading;

namespace Astrarium.Types
{
    public interface ISky
    {
        SkyContext Context { get; }
        event Action Calculated;
        event Action DateTimeSyncChanged;
        void SetDate(double jd);
        void Calculate();
        bool DateTimeSync { get; set; }
        List<Ephemerides> GetEphemerides(CelestialObject body, double from, double to, double step, IEnumerable<string> categories, CancellationToken? cancelToken = null, IProgress<double> progress = null);
        Ephemerides GetEphemerides(CelestialObject body, SkyContext context, IEnumerable<string> categories);
        ICollection<string> GetEphemerisCategories(CelestialObject body);
        ICollection<AstroEvent> GetEvents(double jdFrom, double jdTo, IEnumerable<string> categories, CancellationToken? cancelToken = null);
        ICollection<string> GetEventsCategories();
        CelestialObjectInfo GetInfo(CelestialObject body);
        Constellation GetConstellation(string code);
        ICollection<Tuple<int, int>> ConstellationLines { get; set; }
                
        /// <summary>
        /// Searches celestial objects by string.
        /// </summary>
        /// <param name="searchString">String containing object name.</param>
        /// <param name="filter">Function to filter search results.</param>
        /// <param name="maxCount">Maximal number of objects to be returned.</param>
        /// <returns></returns>
        ICollection<CelestialObject> Search(string searchString, Func<CelestialObject, bool> filter, int maxCount = 50);
    }
}