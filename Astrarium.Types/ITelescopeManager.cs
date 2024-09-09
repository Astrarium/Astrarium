using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
