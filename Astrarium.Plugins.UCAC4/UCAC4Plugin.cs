using Astrarium.Plugins.UCAC4.Controls;
using Astrarium.Plugins.UCAC4.ViewModels;
using Astrarium.Types;

namespace Astrarium.Plugins.UCAC4
{
    public class UCAC4Plugin : AbstractPlugin
    {
        public UCAC4Plugin()
        {
            #region Settings

            DefineSetting("UCAC4", defaultValue: true);
            DefineSetting("UCAC4RootDir", defaultValue: "", isPermanent: true);
            DefineSettingsSection<UCAC4SettingsSection, UCAC4SettingsVM>();

            #endregion Settings

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
