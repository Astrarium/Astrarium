using Astrarium.Config;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.MinorBodies
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            #region Settings

            AddSetting(new SettingItem("Comets", true, "Comets"));
            AddSetting(new SettingItem("CometsLabels", true, "Comets", s => s.Get<bool>("Comets")));

            AddSetting(new SettingItem("Asteroids", true, "Asteroids"));
            AddSetting(new SettingItem("AsteroidsLabels", true, "Asteroids", s => s.Get<bool>("Asteroids")));

            AddSetting(new SettingItem("ColorAsteroidsLabels", Color.FromArgb(10, 44, 37), "Colors"));
            AddSetting(new SettingItem("ColorCometsLabels", Color.FromArgb(78, 84, 99), "Colors"));

            #endregion Settings

            #region Toolbar Integration

            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconAsteroid", "$Settings.Asteroids", new SimpleBinding(settings, "Asteroids", "IsChecked")));
            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconComet", "$Settings.Comets", new SimpleBinding(settings, "Comets", "IsChecked")));

            #endregion Toolbar Integration

            #region Exports

            ExportResourceDictionaries("Images.xaml");

            #endregion
        }
    }
}
