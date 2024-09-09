using Astrarium.Plugins.DeepSky.Controls;
using Astrarium.Plugins.DeepSky.ViewModels;
using Astrarium.Types;
using System.Drawing;

namespace Astrarium.Plugins.DeepSky
{
    public class DeepSkyPlugin : AbstractPlugin
    {
        public DeepSkyPlugin(ISettings settings)
        {
            DefineSetting("DeepSky", true);
            DefineSetting("DeepSkyLabels", true);
            DefineSetting("DeepSkyImages", false, isPermanent: true);
            DefineSetting("DeepSkyImagesFolder", "", isPermanent: true);
            DefineSetting("DeepSkyHideOutline", false);

            // Colors
            DefineSetting("ColorDeepSkyOutline", Color.DodgerBlue);
            DefineSetting("ColorDeepSkyLabel", Color.DodgerBlue);

            // Fonts
            DefineSetting("DeepSkyLabelsFont", new Font("Arial", 9, FontStyle.Italic));

            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconDeepSky", "$Settings.DeepSky", new SimpleBinding(settings, "DeepSky", "IsChecked")));

            DefineSettingsSection<DeepSkySettingsSection, DeepSkySettingsViewModel>();

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
