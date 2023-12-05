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
            DefineSetting("ColorMilkyWay", Color.FromArgb(20, 20, 20));
            DefineSettingsSection<MilkyWaySettingsSection, SettingsViewModel>();
        }
    }
}
