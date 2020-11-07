using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Astrarium.Plugins.Constellations
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            SettingItems.Add("Constellations", new SettingItem("ConstBorders", true));
            SettingItems.Add("Constellations", new SettingItem("ConstLabels", true));
            SettingItems.Add("Constellations", new SettingItem("ConstLabelsType", ConstellationsRenderer.LabelType.InternationalName, s => s.Get<bool>("ConstLabels")));

            // Colors
            SettingItems.Add("Colors", new SettingItem("ColorConstBorders", new SkyColor(64, 32, 32)));
            SettingItems.Add("Colors", new SettingItem("ColorConstLabels", new SkyColor(64, 32, 32)));

            // Fonts
            SettingItems.Add("Fonts", new SettingItem("ConstLabelsFont", new Font(FontFamily.GenericSansSerif, 32)));

            ToolbarItems.Add("Constellations", new ToolbarToggleButton("IconConstBorders", "$Settings.ConstBorders", new SimpleBinding(settings, "ConstBorders", "IsChecked")));
            ToolbarItems.Add("Constellations", new ToolbarToggleButton("IconConstLabels", "$Settings.ConstLabels", new SimpleBinding(settings, "ConstLabels", "IsChecked")));

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
