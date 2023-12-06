using Astrarium.Plugins.Atmosphere.Controls;
using Astrarium.Types;

namespace Astrarium.Plugins.Atmosphere
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            DefineSetting("Atmosphere", true);
            DefineSetting("LightPollution", false);

            DefineSetting("LightPollutionAltitude", 60m);
            DefineSetting("LightPollutionTone", 30m);
            DefineSetting("LightPollutionIntensity", 30m);

            DefineSettingsSection<AtmosphereSettingsSection, SettingsViewModel>();

            ExportResourceDictionaries("Images.xaml");

            ToolbarItems.Add("Ground", new ToolbarToggleButton("IconAtmosphere", "$Settings.Atmosphere", new SimpleBinding(settings, "Atmosphere", "IsChecked")));
        }
    }
}
