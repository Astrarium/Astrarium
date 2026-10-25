using Astrarium.Plugins.Hipparcos.Controls;
using Astrarium.Types;
using System.Drawing;

namespace Astrarium.Plugins.Hipparcos
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            DefineSetting("Stars", true);

            DefineSetting("StarsLabels", true);
            DefineSetting("StarsProperNames", true);
            DefineSetting("StarsColors", true);

            DefineSetting("StarsMarkVariables", false);
            DefineSetting("StarsMarkMultiples", false);

            DefineSetting("ColorStarsLabels", Color.Silver);
            DefineSetting("StarsLabelsFont", new Font(FontFamily.GenericSerif, 10));

            ExportResourceDictionaries("Images.xaml");

            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconStar", "$Settings.Stars", settings, "Stars"));

            DefineSettingsSection<HipparcosSettingsSection, SettingsViewModel>();
        }
    }
}
