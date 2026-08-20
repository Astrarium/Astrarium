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

    /// <summary>
    /// Default stub for the interface, in case when telescope control is not available
    /// </summary>
    public class TelescopeManagerStub : ITelescopeManager
    {
        public bool IsTelescopeAvailable => false;
        public bool IsTelescopeConnected => false;
        public event Action TelescopeConnectionChanged;
        public void SlewToCoordinates(CrdsEquatorial eq) { }
    }
}
