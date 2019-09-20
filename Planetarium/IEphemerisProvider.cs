using System;
using System.Collections.Generic;
using System.Threading;
using Planetarium.Objects;
using Planetarium.Types;

namespace Planetarium
{
    public interface IEphemerisProvider
    {
        List<List<Ephemeris>> GetEphemerides(CelestialObject body, double from, double to, double step, IEnumerable<string> categories, CancellationToken? cancelToken = null, IProgress<double> progress = null);
        ICollection<string> GetEphemerisCategories(CelestialObject body);
    }
}