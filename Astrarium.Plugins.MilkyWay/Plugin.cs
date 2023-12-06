using Astrarium.Plugins.MilkyWay.Controls;
using Astrarium.Types;
using System.Drawing;

namespace Astrarium.Plugins.MilkyWay
{
    public class Plugin : AbstractPlugin
    {
        public Plugin()
        {
            DefineSetting("MilkyWay", true);
            DefineSetting("MilkyWayDimOnZoom", true);
            DefineSettingsSection<MilkyWaySettingsSection, SettingsViewModel>();
        }
    }
}
