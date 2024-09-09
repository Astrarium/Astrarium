using Astrarium.Plugins.Supernovae.Controls;
using Astrarium.Types;

namespace Astrarium.Plugins.Supernovae
{
    public class SupernovaePlugin : AbstractPlugin
    {
        public SupernovaePlugin()
        {
            DefineSetting("Supernovae", true);
            DefineSetting("SupernovaeLabels", true);
            DefineSetting("SupernovaeDrawAll", false);
            DefineSettingsSection<SupernovaeSettingsSection, SettingsViewModel>();
            ExportResourceDictionaries("Images.xaml");
        }
    }
}
