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

            AddSetting(new SettingItem("ConstLabels", true, "Constellations"));
            AddSetting(new SettingItem("ConstLabelsType", ConstellationsRenderer.LabelType.InternationalName, "Constellations", s => s.Get<bool>("ConstLabels")));
            
            AddSetting(new SettingItem("ConstBorders", true, "Constellations"));

            AddSetting(new SettingItem("ColorConstBorders", Color.FromArgb(64, 32, 32)));
            AddSetting(new SettingItem("ColorConstLabels", Color.FromArgb(64, 32, 32)));
            
            #endregion Settings

            #region Toolbar Integration
            
            AddToolbarItem(new ToolbarButton("Constellation Borders", "IconConstBorders", settings, "ConstBorders", "Constellations"));

            #endregion Toolbar Integration

            #region Exports

            ExportResourceDictionaries("Images.xaml");

            #endregion
        }
    }
}
