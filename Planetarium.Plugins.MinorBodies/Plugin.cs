using Planetarium.Config;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.MinorBodies
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            #region Settings

            AddSetting(new SettingItem("Comets", true, "Comets"));
            AddSetting(new SettingItem("CometsLabels", true, "Comets", s => s.Get<bool>("Comets")));

            AddSetting(new SettingItem("Asteroids", true, "Asteroids"));
            AddSetting(new SettingItem("AsteroidsLabels", true, "Asteroids", s => s.Get<bool>("Asteroids")));

            #endregion Settings

            #region Toolbar Integration

            AddToolbarItem(new ToolbarButton("Asteroids", "IconAsteroid", settings, "Asteroids", "Objects"));
            AddToolbarItem(new ToolbarButton("Comets", "IconComet", settings, "Comets", "Objects"));

            #endregion Toolbar Integration

            #region Exports

            ExportResourceDictionaries("Images.xaml");

            #endregion
        }
    }
}
