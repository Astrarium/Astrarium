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
            SettingItems.Add("Comets", new SettingItem("Comets", true));
            SettingItems.Add("Comets", new SettingItem("CometsLabels", true, s => s.Get<bool>("Comets")));
            SettingItems.Add("Asteroids", new SettingItem("Asteroids", true));
            SettingItems.Add("Asteroids", new SettingItem("AsteroidsLabels", true, s => s.Get<bool>("Asteroids")));
            SettingItems.Add("Colors", new SettingItem("ColorAsteroidsLabels", Color.FromArgb(10, 44, 37)));
            SettingItems.Add("Colors", new SettingItem("ColorCometsLabels", Color.FromArgb(78, 84, 99)));

            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconAsteroid", "$Settings.Asteroids", new SimpleBinding(settings, "Asteroids", "IsChecked")));
            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconComet", "$Settings.Comets", new SimpleBinding(settings, "Comets", "IsChecked")));

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
