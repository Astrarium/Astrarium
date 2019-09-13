using Planetarium.Types;
using Planetarium.Types.Config.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.Tycho2
{
    public class Tycho2Plugin : AbstractPlugin
    {
        public Tycho2Plugin()
        {
            #region Settings

            AddSetting("Tycho2", 
                defaultValue: true, 
                sectionName: "Tycho 2");                             // setting is placed into "Tycho 2" section 

            AddSetting("Tycho2Path", 
                defaultValue: "", 
                sectionName: "Tycho 2",                              // setting is placed into "Tycho 2" section
                controlType: typeof(FolderPickerSettingControl),     // type of UI editor
                enabledCondition: s => s.Get<bool>("Tycho2")         // setting is enabled when "Tycho2" setting is ON
            );

            #endregion Settings
        }
    }
}
