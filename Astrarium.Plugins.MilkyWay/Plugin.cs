using Astrarium.Plugins.MilkyWay.Controls;
using Astrarium.Types;

namespace Astrarium.Plugins.MilkyWay
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            DefineSetting("MilkyWay", true);
            DefineSetting("MilkyWayDimOnZoom", true);
            DefineSettingsSection<MilkyWaySettingsSection, SettingsViewModel>();

            ExportResourceDictionaries("Images.xaml");

            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconMilkyWay", "$Settings.MilkyWay", new SimpleBinding(settings, "MilkyWay", "IsChecked")));
        }
    }
}
