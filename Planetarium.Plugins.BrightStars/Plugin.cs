using Planetarium.Config;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
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
            AddSetting(new SettingItem("ConstLines", true, "Constellations"));

            AddSetting(new SettingItem("ColorConstLines", new SkyColor() { Night = Color.FromArgb(64, 64, 64), Day = Color.FromArgb(74, 142, 213), Red = Color.DarkRed, White = Color.FromArgb(150, 150, 150) }));

            #endregion Settings

            AddToolbarItem(new ToolbarButton("Stars", "IconStars", settings, "Stars", "Objects"));
            AddToolbarItem(new ToolbarButton("Constellation Lines", "IconConstLines", settings, "ConstLines", "Constellations"));

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
