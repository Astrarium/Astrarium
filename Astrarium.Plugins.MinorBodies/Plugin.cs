using Astrarium.Plugins.MinorBodies.Controls;
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
            SettingItems.Add("Comets", new SettingItem("CometsLabelsMag", false, s => s.Get<bool>("Comets") && s.Get<bool>("CometsLabels")));
            SettingItems.Add("Comets", new SettingItem("CometsDrawAll", false, s => s.Get<bool>("Comets")));
            SettingItems.Add("Comets", new SettingItem("CometsDrawAllMagLimit", (decimal)10, typeof(UpDownSettingControl), s => s.Get<bool>("Comets") && s.Get<bool>("CometsDrawAll")));

            SettingItems.Add("Asteroids", new SettingItem("Asteroids", true));
            SettingItems.Add("Asteroids", new SettingItem("AsteroidsLabels", true, s => s.Get<bool>("Asteroids")));            
            SettingItems.Add("Asteroids", new SettingItem("AsteroidsLabelsMag", false, s => s.Get<bool>("Asteroids") && s.Get<bool>("AsteroidsLabels")));
            SettingItems.Add("Asteroids", new SettingItem("AsteroidsDrawAll", false, s => s.Get<bool>("Asteroids")));
            SettingItems.Add("Asteroids", new SettingItem("AsteroidsDrawAllMagLimit", (decimal)10, typeof(UpDownSettingControl), s => s.Get<bool>("Asteroids") && s.Get<bool>("AsteroidsDrawAll")));

            SettingItems.Add("Colors", new SettingItem("ColorAsteroidsLabels", new SkyColor(10, 44, 37)));
            SettingItems.Add("Colors", new SettingItem("ColorCometsLabels", new SkyColor(78, 84, 99)));

            SettingItems.Add("Fonts", new SettingItem("AsteroidsLabelsFont", new Font("Arial", 8)));
            SettingItems.Add("Fonts", new SettingItem("CometsLabelsFont", new Font("Arial", 8)));

            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconAsteroid", "$Settings.Asteroids", new SimpleBinding(settings, "Asteroids", "IsChecked")));
            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconComet", "$Settings.Comets", new SimpleBinding(settings, "Comets", "IsChecked")));

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
