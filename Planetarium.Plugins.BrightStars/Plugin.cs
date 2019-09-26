using Planetarium.Config;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Planetarium.Plugins.BrightStars
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            #region Settings

            AddSetting(new SettingItem("Stars", true, "Stars"));
            AddSetting(new SettingItem("StarsLabels", true, "Stars", s => s.Get<bool>("Stars")));
            AddSetting(new SettingItem("StarsProperNames", true, "Stars", s => s.Get<bool>("Stars") && s.Get<bool>("StarsLabels")));

            #endregion Settings

            AddToolbarItem(new ToolbarButton("Stars", "IconStars", settings, "Stars", "Objects"));

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
