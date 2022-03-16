using Astrarium.Plugins.MilkyWay.Controls;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.MilkyWay
{
    public class Plugin : AbstractPlugin
    {
        public Plugin()
        {
            DefineSetting("MilkyWay", true);
            DefineSetting("ColorMilkyWay", new SkyColor(20, 20, 20));
            DefineSettingsSection<MilkyWaySettingsSection, SettingsViewModel>();
        }
    }
}
