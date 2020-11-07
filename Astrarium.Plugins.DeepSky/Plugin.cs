using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Astrarium.Plugins.DeepSky
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            SettingItems.Add("DeepSky", new SettingItem("DeepSky", true));
            SettingItems.Add("DeepSky", new SettingItem("DeepSkyLabels", true, s => s.Get<bool>("DeepSky")));
            SettingItems.Add("DeepSky", new SettingItem("DeepSkyOutlines", true, s => s.Get<bool>("DeepSky")));
            
            // Colors
            SettingItems.Add("Colors", new SettingItem("ColorDeepSkyOutline", new SkyColor(50, 50, 50)));
            SettingItems.Add("Colors", new SettingItem("ColorDeepSkyLabel", new SkyColor(0, 64, 128)));

            // Fonts
            SettingItems.Add("Fonts", new SettingItem("DeepSkyLabelsFont", new Font("Arial", 7)));

            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconDeepSky", "$Settings.DeepSky", new SimpleBinding(settings, "DeepSky", "IsChecked")));

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
