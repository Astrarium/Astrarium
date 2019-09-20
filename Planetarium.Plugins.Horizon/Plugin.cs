using Planetarium.Config;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.Horizon
{
    public class Plugin : AbstractPlugin
    {
        public Plugin()
        {
            #region Settings

            AddSetting(new SettingItem("Ground", true, "Grids"));
            AddSetting(new SettingItem("HorizonLine", true, "Grids"));
            AddSetting(new SettingItem("LabelCardinalDirections", true, "Grids", s => s.Get<bool>("HorizonLine")));

            #endregion Settings
        }
    }
}
