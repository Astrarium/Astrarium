using System;
using System.Collections.Generic;
using System.Threading;
using Astrarium.Objects;
using Astrarium.Types;

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
        List<List<Ephemeris>> GetEphemerides(CelestialObject body, double from, double to, double step, IEnumerable<string> categories, CancellationToken? cancelToken = null, IProgress<double> progress = null);
        ICollection<string> GetEphemerisCategories(CelestialObject body);
        ICollection<AstroEvent> GetEvents(double jdFrom, double jdTo, IEnumerable<string> categories, CancellationToken? cancelToken = null);
        ICollection<string> GetEventsCategories();
        CelestialObjectInfo GetInfo(CelestialObject body);
        Constellation GetConstellation(string code);
        ICollection<SearchResultItem> Search(string searchString, Func<CelestialObject, bool> filter);
    }
}