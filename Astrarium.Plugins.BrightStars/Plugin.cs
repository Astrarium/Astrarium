using Astrarium.Config;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Astrarium.Plugins.BrightStars
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            SettingItems.Add("Stars", new SettingItem("Stars", true));
            SettingItems.Add("Stars", new SettingItem("StarsLabels", true, s => s.Get<bool>("Stars")));
            SettingItems.Add("Stars", new SettingItem("StarsProperNames", true, s => s.Get<bool>("Stars") && s.Get<bool>("StarsLabels")));
            SettingItems.Add("Stars", new SettingItem("StarsColors", true, s => s.Get<bool>("Stars")));
            SettingItems.Add("Constellations", new SettingItem("ConstLines", true));
            SettingItems.Add("Colors", new SettingItem("ColorConstLines", Color.FromArgb(64, 64, 64)));
            SettingItems.Add("Colors", new SettingItem("ColorStarsLabels", Color.FromArgb(64, 64, 64)));

            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconStar", "$Settings.Stars", new SimpleBinding(settings, "Stars", "IsChecked")));
            ToolbarItems.Add("Constellations", new ToolbarToggleButton("IconConstLines", "$Settings.ConstLines", new SimpleBinding(settings, "ConstLines", "IsChecked")));

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
