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

            SettingItems.Add("Tycho 2", new SettingItem("Tycho2", defaultValue: true));

            #endregion Settings

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
