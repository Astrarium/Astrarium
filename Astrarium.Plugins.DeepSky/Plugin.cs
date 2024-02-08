using Astrarium.Plugins.DeepSky.Controls;
using Astrarium.Plugins.DeepSky.ViewModels;
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
            DefineSetting("DeepSky", true);
            DefineSetting("DeepSkyLabels", true);
            DefineSetting("DeepSkyImages", false);
            DefineSetting("DeepSkyImagesFolder", "");

            // Colors
            DefineSetting("ColorDeepSkyOutline", Color.FromArgb(0, 50, 75));
            DefineSetting("ColorDeepSkyLabel", Color.FromArgb(0, 64, 128));

            // Fonts
            DefineSetting("DeepSkyLabelsFont", new Font("Arial", 7));

            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconDeepSky", "$Settings.DeepSky", new SimpleBinding(settings, "DeepSky", "IsChecked")));

            DefineSettingsSection<DeepSkySettingsSection, DeepSkySettingsViewModel>();

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
