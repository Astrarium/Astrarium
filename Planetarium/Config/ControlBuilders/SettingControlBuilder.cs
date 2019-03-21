using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Planetarium.Config.ControlBuilders
{
    public abstract class SettingControlBuilder
    {
        public abstract FrameworkElement Build(ISettings settings, SettingConfigItem item);
    }
}
