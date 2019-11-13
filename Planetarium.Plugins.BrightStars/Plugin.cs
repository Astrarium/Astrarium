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

            AddSetting(new SettingItem("ColorConstLines", Color.FromArgb(64, 64, 64), "Colors"));
            AddSetting(new SettingItem("ColorStarsLabels", Color.FromArgb(64, 64, 64), "Colors"));

            #endregion Settings

            AddToolbarItem(new ToolbarToggleButton("Settings.Stars", "IconStars", new SimpleBinding(settings, "Stars"), "Objects"));
            AddToolbarItem(new ToolbarToggleButton("Settings.ConstLines", "IconConstLines", new SimpleBinding(settings, "ConstLines"), "Constellations"));

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
