using Astrarium.Plugins.ASCOM.ViewModels;
using Astrarium.Plugins.RestAPI.Controls;
using Astrarium.Types;

namespace Astrarium.Plugins.RestAPI
{
    public class Plugin : AbstractPlugin
    {
        private readonly ISettings settings;
        private readonly IRestAPIServer server;

        public Plugin(IRestAPIServer server, ISettings settings)
        {
            this.server = server;
            this.settings = settings;

            this.settings.SettingValueChanged += Settings_SettingValueChanged;

            DefineSetting(SettingsNames.RestAPIServerEnabled, false);
            DefineSetting(SettingsNames.RestAPIServerPort, 9000m);
            DefineSettingsSection<RestAPISettingsSection, RestAPISettingsViewModel>();
            ExportResourceDictionaries("Images.xaml");
        }

        private void Settings_SettingValueChanged(string settingName, object newValue, object oldValue)
        {
            if (settingName == SettingsNames.RestAPIServerEnabled)
            {
                server.Stop();
                if (newValue is true)
                {
                    server.Start();
                }
            }
        }

        public override void Initialize()
        {
            if (settings.Get(SettingsNames.RestAPIServerEnabled))
            {
                server.Start();
            }
        }
    }
}
