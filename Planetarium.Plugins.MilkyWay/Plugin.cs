﻿using Planetarium.Config;
using Planetarium.Types;
using System;
using System.Collections.Generic;
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

            #endregion Settings
        }
    }
}