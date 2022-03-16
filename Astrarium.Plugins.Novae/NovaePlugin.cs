using Astrarium.Plugins.Novae.Controls;
using Astrarium.Types;
using System;
using System.IO;
using System.Reflection;

namespace Astrarium.Plugins.Novae
{
    public class NovaePlugin : AbstractPlugin
    {
        public NovaePlugin()
        {
            DefineSetting("Novae", true);
            DefineSetting("NovaeLabels", true);
            DefineSettingsSection<NovaeSettingsSection, SettingsViewModel>();
            ExportResourceDictionaries("Images.xaml");
        }
    }
}
