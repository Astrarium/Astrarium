using Astrarium.Plugins.MilkyWay.Controls;
using Astrarium.Types;
using System.Drawing;

namespace Astrarium.Plugins.MilkyWay
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            DefineSetting("MilkyWay", true);
            DefineSetting("MilkyWayDimOnZoom", true);
            DefineSettingsSection<MilkyWaySettingsSection, SettingsViewModel>();

            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconMilkyWay", "$Settings.MilkyWay", new SimpleBinding(settings, "MilkyWay", "IsChecked")));
            ExportResourceDictionaries("Images.xaml");
        }
    }
}
