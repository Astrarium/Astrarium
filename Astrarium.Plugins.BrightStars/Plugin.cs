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
            #region Settings

            AddSetting(new SettingItem("Stars", true, "Stars"));
            AddSetting(new SettingItem("StarsLabels", true, "Stars", s => s.Get<bool>("Stars")));
            AddSetting(new SettingItem("StarsProperNames", true, "Stars", s => s.Get<bool>("Stars") && s.Get<bool>("StarsLabels")));
            AddSetting(new SettingItem("StarsColors", true, "Stars", s => s.Get<bool>("Stars")));

            AddSetting(new SettingItem("ConstLines", true, "Constellations"));

            AddSetting(new SettingItem("ColorConstLines", Color.FromArgb(64, 64, 64), "Colors"));
            AddSetting(new SettingItem("ColorStarsLabels", Color.FromArgb(64, 64, 64), "Colors"));

            #endregion Settings

            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconStar", "$Settings.Stars", new SimpleBinding(settings, "Stars", "IsChecked")));
            ToolbarItems.Add("Constellations", new ToolbarToggleButton("IconConstLines", "$Settings.ConstLines", new SimpleBinding(settings, "ConstLines", "IsChecked")));

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
