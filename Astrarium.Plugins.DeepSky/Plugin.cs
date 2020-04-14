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

namespace Astrarium.Plugins.DeepSky
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            #region Settings

            SettingItems.Add("DeepSky", new SettingItem("DeepSky", true, "Deep Sky Objects"));
            SettingItems.Add("DeepSky", new SettingItem("DeepSkyLabels", true, "Deep Sky Objects", s => s.Get<bool>("DeepSky")));
            SettingItems.Add("DeepSky", new SettingItem("DeepSkyOutlines", true, "Deep Sky Objects", s => s.Get<bool>("DeepSky")));

            SettingItems.Add("Colors", new SettingItem("ColorDeepSkyOutline", Color.FromArgb(50, 50, 50), "Colors"));
            SettingItems.Add("Colors", new SettingItem("ColorDeepSkyLabel", Color.FromArgb(0, 64, 128), "Colors"));

            #endregion Settings

            #region Toolbar Integration

            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconDeepSky", "$Settings.DeepSky", new SimpleBinding(settings, "DeepSky", "IsChecked")));

            #endregion Toolbar Integration

            #region Exports

            ExportResourceDictionaries("Images.xaml");

            #endregion
        }
    }
}
