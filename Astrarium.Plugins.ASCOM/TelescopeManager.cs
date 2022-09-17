using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.ASCOM
{
    [Singleton(typeof(ITelescopeManager))]
    public class TelescopeManager : ITelescopeManager
    {
        private readonly IAscomProxy proxy = null;

        public TelescopeManager()
        {
            proxy = Ascom.Proxy;
            proxy.PropertyChanged += Proxy_PropertyChanged;
        }

        private void Proxy_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAscomProxy.IsConnected))
            {
                TelescopeConnectionChanged?.Invoke();
            }
        }

        public bool IsTelescopeAvailable => proxy.IsAscomPlatformInstalled;

        public bool IsTelescopeConnected => proxy.IsConnected;

        public event Action TelescopeConnectionChanged;

        public void SlewToCoordinates(CrdsEquatorial eq)
        {
            proxy.Slew(eq);
        }
    }
}
