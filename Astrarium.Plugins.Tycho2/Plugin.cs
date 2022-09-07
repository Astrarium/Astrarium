using Astrarium.Plugins.Tycho2.Controls;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Tycho2
{
    public class Plugin : AbstractPlugin
    {
        public Plugin()
        {
            #region Settings

            DefineSetting("Tycho2", defaultValue: true);
            DefineSetting("Tycho2RootDir", defaultValue: "", isPermanent: true);
            DefineSettingsSection<Tycho2SettingsSection, Tycho2SettingsViewModel>();

            #endregion Settings

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
