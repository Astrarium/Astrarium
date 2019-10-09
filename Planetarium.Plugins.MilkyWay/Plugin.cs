using Planetarium.Config;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.MilkyWay
{
    public class Plugin : AbstractPlugin
    {
        public Plugin()
        {
            #region Settings

            AddSetting(new SettingItem("MilkyWay", true, "Grids"));

            AddSetting(new SettingItem("ColorMilkyWay", new SkyColor() { Night = Color.FromArgb(20, 20, 20), Day = Color.FromArgb(116, 184, 254), Red = Color.FromArgb(20, 0, 0), White = Color.FromArgb(230, 230, 230) }));

            #endregion Settings
        }
    }
}
