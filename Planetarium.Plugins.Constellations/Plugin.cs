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

namespace Planetarium.Plugins.Constellations
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            #region Settings

            AddSetting(new SettingItem("ConstBorders", true, "Constellations"));
            AddSetting(new SettingItem("ConstLabels", true, "Constellations"));
            AddSetting(new SettingItem("ConstLabelsType", ConstellationsRenderer.LabelType.InternationalName, "Constellations", s => s.Get<bool>("ConstLabels")));
           
            AddSetting(new SettingItem("ColorConstBorders", Color.FromArgb(64, 32, 32), "Colors"));
            AddSetting(new SettingItem("ColorConstLabels", Color.FromArgb(64, 32, 32), "Colors"));
            
            #endregion Settings

            #region Toolbar Integration
            
            AddToolbarItem(new ToolbarToggleButton("Settings.ConstBorders", "IconConstBorders", new SimpleBinding(settings, "ConstBorders"), "Constellations"));
            AddToolbarItem(new ToolbarToggleButton("Settings.ConstLabels", "IconConstLabels", new SimpleBinding(settings, "ConstLabels"), "Constellations"));

            #endregion Toolbar Integration

            #region Exports

            ExportResourceDictionaries("Images.xaml");

            #endregion
        }
    }
}
