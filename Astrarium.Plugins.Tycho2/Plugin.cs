using Astrarium.Config;
using Astrarium.Types;
using Astrarium.Types.Config.Controls;
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

            AddSetting(new SettingItem(
                "Tycho2", 
                defaultValue: true, 
                sectionName: "Tycho 2"));                             // setting is placed into "Tycho 2" section 

            AddSetting(new SettingItem(
                "Tycho2Path", 
                defaultValue: "", 
                sectionName: "Tycho 2",                              // setting is placed into "Tycho 2" section
                controlType: typeof(FolderPickerSettingControl),     // type of UI editor
                enabledCondition: s => s.Get<bool>("Tycho2")         // setting is enabled when "Tycho2" setting is ON
            ));

            #endregion Settings

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
