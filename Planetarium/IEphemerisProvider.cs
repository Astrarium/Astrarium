using System.Collections.Generic;
using Planetarium.Objects;

namespace Planetarium
{
    public interface IEphemerisProvider
    {
        List<List<Ephemeris>> GetEphemerides(CelestialObject body, double from, double to, double step, IEnumerable<string> categories);
        ICollection<T> GetEphemeris<T>(CelestialObject body, double from, double to, double step, string ephemKey);
        T GetEphemeris<T>(CelestialObject body, double jd, string ephemKey);
        ICollection<string> GetEphemerisCategories(CelestialObject body);
    }
}