using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium
{
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
