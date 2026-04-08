using Astrarium.Algorithms;
using System;

namespace Astrarium.Types
{
    /// <summary>
    /// Interface to access telescope from application plugins
    /// </summary>
    public interface ITelescopeManager
    {
        void SlewToCoordinates(CrdsEquatorial eq);
        bool IsTelescopeAvailable { get; }
        bool IsTelescopeConnected { get; }
        event Action TelescopeConnectionChanged;
    }
}
