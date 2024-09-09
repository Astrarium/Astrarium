using System;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Satellites
{
    public interface IOrbitalElementsUpdater
    {
        Task<bool> UpdateOrbitalElements(TLESource tleSource, bool silent);
        event Action<TLESource> OrbitalElementsUpdated;
    }
}