using Astrarium.Plugins.UCAC4.Controls;
using Astrarium.Plugins.UCAC4.ViewModels;
using Astrarium.Types;
using System.ComponentModel;

namespace Astrarium.Plugins.UCAC4
{
    public class UCAC4Plugin : AbstractPlugin
    {
        public UCAC4Plugin()
        {
            #region Settings

            DefineSetting("UCAC4", defaultValue: true);
            DefineSetting("UCAC4RootDir", defaultValue: "", isPermanent: true);
            DefineSetting("UCAC4InMemoryIndex", false);

            DefineSetting("UCAC4UseMagLimit", defaultValue: true);
            DefineSetting("UCAC4MagLimit", 6.5m);
            DefineSetting("UCAC4HideUnknownMag", defaultValue: true);

            DefineSettingsSection<UCAC4SettingsSection, UCAC4SettingsVM>();

            #endregion Settings

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
