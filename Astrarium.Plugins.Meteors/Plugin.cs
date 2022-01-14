using Astrarium.Types;
using System;

namespace Astrarium.Plugins.Meteors
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            SettingItems.Add("Meteors", new SettingItem("Meteors", true));
            SettingItems.Add("Meteors", new SettingItem("MeteorsOnlyActive", true, s => s.Get<bool>("Meteors")));
            SettingItems.Add("Meteors", new SettingItem("MeteorsLabels", true, s => s.Get<bool>("Meteors")));
            SettingItems.Add("Colors", new SettingItem("ColorMeteors", new SkyColor(140, 16, 53)));

            MenuItems.Add(MenuItemPosition.MainMenuTools,
                new MenuItem("$Astrarium.Plugins.Meteors.ToolsMenu",
                new Command(() => ViewManager.ShowWindow<MeteorShowersVM>(isSingleInstance: true))));

            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconMeteor", "$Settings.Meteors", new SimpleBinding(settings, "Meteors", "IsChecked")));

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
