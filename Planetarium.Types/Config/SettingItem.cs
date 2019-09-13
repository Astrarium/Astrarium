using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Planetarium.Types;

namespace Planetarium.Config
{
    public class SettingItem
    {
        public SettingItem() { }
        public string Name { get; set; }
        public object DefaultValue { get; set; }
        public string Section { get; set; }
        public Func<ISettings, bool> EnabledCondition { get; set; }
        public Type ControlType { get; set; }
    }
}
