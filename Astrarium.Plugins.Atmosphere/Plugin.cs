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

            DefineSetting("Fog", false);
            DefineSetting("FogAltitude", 10m);
            DefineSetting("FogSpreading", 10m);
            DefineSetting("FogIntensity", 30m);

            DefineSetting("Refraction", false);
            DefineSetting("RefractionTemperature", 10m);
            DefineSetting("RefractionPressure", 1010m);

            DefineSetting("Extinction", false);
            DefineSetting("ExtinctionCoefficient", 0.3m);
            DefineSetting("AirmassModel", AirmassModel.Hardie);

            DefineSettingsSection<AtmosphereSettingsSection, SettingsViewModel>();

            ExportResourceDictionaries("Images.xaml");

            ToolbarItems.Add("Ground", new ToolbarToggleButton("IconAtmosphere", "$Settings.Atmosphere", new SimpleBinding(settings, "Atmosphere", "IsChecked")));
            ToolbarItems.Add("Ground", new ToolbarToggleButton("IconRefraction", "$Settings.Refraction", new SimpleBinding(settings, "Refraction", "IsChecked")));
            ToolbarItems.Add("Ground", new ToolbarToggleButton("IconExtinction", "$Settings.Extinction", new SimpleBinding(settings, "Extinction", "IsChecked")));
        }
    }
}
