using Astrarium.Plugins.BrightStars.Controls;
using Astrarium.Types;
using System.Drawing;

namespace Astrarium.Plugins.BrightStars
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            DefineSetting("Stars", true);
            DefineSetting("StarsScalingFactor", 1m);
            DefineSetting("StarsLabels", true);
            DefineSetting("StarsProperNames", true);
            DefineSetting("StarsColors", true);

            DefineSetting("ColorStarsLabels", new SkyColor(64, 64, 64));
            DefineSetting("StarsLabelsFont", new Font("Arial", 8));

            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconStar", "$Settings.Stars", new SimpleBinding(settings, "Stars", "IsChecked")));

            DefineSettingsSection<BrightStarsSettingsSection, SettingsViewModel>();

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
