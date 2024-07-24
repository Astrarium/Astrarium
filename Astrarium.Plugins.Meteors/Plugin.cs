using Astrarium.Plugins.Meteors.Controls;
using Astrarium.Types;
using System;
using System.Drawing;

namespace Astrarium.Plugins.Meteors
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            DefineSetting("Meteors", true);
            DefineSetting("MeteorsActivityClassLimit", MeteorActivityClass.IV);
            DefineSetting("MeteorsOnlyActive", true);
            DefineSetting("MeteorsLabels", true);
            DefineSetting("MeteorsLabelsType", MeteorLabelType.Name);
            DefineSetting("ColorMeteors", Color.FromArgb(140, 16, 53));

            DefineSetting("MeteorsLabelsFont", new Font("Arial", 8));

            MenuItems.Add(MenuItemPosition.MainMenuTools,
                new MenuItem("$Astrarium.Plugins.Meteors.ToolsMenu",
                new Command(() => ViewManager.ShowWindow<MeteorShowersVM>(ViewFlags.SingleInstance))));

            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconMeteor", "$Settings.Meteors", new SimpleBinding(settings, "Meteors", "IsChecked")));

            DefineSettingsSection<MeteorsSettingsSection, SettingsViewModel>();

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
