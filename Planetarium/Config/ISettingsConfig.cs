using Planetarium.Config.ControlBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Config
{
    public interface ISettingsConfig : IEnumerable<SettingConfigItem>
    {
        SettingControlBuilder GetBuilder(Type settingType);
    }
}
