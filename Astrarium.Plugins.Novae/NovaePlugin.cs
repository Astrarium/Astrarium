using Astrarium.Types;
using System;
using System.IO;
using System.Reflection;

namespace Astrarium.Plugins.Novae
{
    public class NovaePlugin : AbstractPlugin
    {
        public NovaePlugin(ISettings settings)
        {
            SettingItems.Add("Novae", new SettingItem("Novae", true));
            SettingItems.Add("Novae", new SettingItem("NovaeLabels", true, s => s.Get<bool>("Novae")));


            ExportResourceDictionaries("Images.xaml");
        }
    }
}
