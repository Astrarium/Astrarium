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

namespace Astrarium.Plugins.Constellations
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            #region Settings

            SettingItems.Add("Constellations", new SettingItem("ConstBorders", true, "Constellations"));
            SettingItems.Add("Constellations", new SettingItem("ConstLabels", true, "Constellations"));
            SettingItems.Add("Constellations", new SettingItem("ConstLabelsType", ConstellationsRenderer.LabelType.InternationalName, "Constellations", s => s.Get<bool>("ConstLabels")));

            SettingItems.Add("Colors", new SettingItem("ColorConstBorders", Color.FromArgb(64, 32, 32), "Colors"));
            SettingItems.Add("Colors", new SettingItem("ColorConstLabels", Color.FromArgb(64, 32, 32), "Colors"));
            
            #endregion Settings

            #region Toolbar Integration
            
            ToolbarItems.Add("Constellations", new ToolbarToggleButton("IconConstBorders", "$Settings.ConstBorders", new SimpleBinding(settings, "ConstBorders", "IsChecked")));
            ToolbarItems.Add("Constellations", new ToolbarToggleButton("IconConstLabels", "$Settings.ConstLabels", new SimpleBinding(settings, "ConstLabels", "IsChecked")));

            #endregion Toolbar Integration

            #region Exports

            ExportResourceDictionaries("Images.xaml");

            #endregion
        }
    }
}
