using Astrarium.Algorithms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.ASCOM
{
    /// <summary>
    /// Proxy interface to communicate with ASCOM platform
    /// </summary>
    public class NoAscomProxy : IAscomProxy
    {
        public bool IsAscomPlatformInstalled => false;
        public int PollingPeriod { get; set; }
        public bool IsConnected => false;
        public bool IsSlewing => false;
        public bool AtHome => false;
        public bool AtPark => false;
        public bool IsTracking => false;
        public CrdsEquatorial Position => new CrdsEquatorial();
        public string TelescopeName { get; }
        public string Connect(string telescopeId) => null;
        public void Disconnect() { }
        public void SetLocation(CrdsGeographical geo) { }
        public void SetDateTime(DateTime utc) { }
        public void Slew(CrdsEquatorial eq) { }
        public void Sync(CrdsEquatorial eq) { }
        public void ShowSetupDialog() { }
        public void AbortSlewing() { }
        public void FindHome() { }
        public void Park() { }
        public void Unpark() { }
        public void SwitchTracking() { }
        public AscomInfo Info { get; }

        public event Action<string> OnMessageShow;
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
