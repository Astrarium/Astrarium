using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Astrarium.Types;

namespace Astrarium.Types
{
    public class SettingItem
    { 
        public SettingItem(string name, object defaultValue) :
            this(name, defaultValue, null, null) { }

        public SettingItem(string name, object defaultValue, Type controlType) : 
            this(name, defaultValue, controlType, null) { }

        public SettingItem(string name, object defaultValue, Func<ISettings, bool> enabledCondition) :
            this(name, defaultValue, null, enabledCondition) { }

        public SettingItem(string name, object defaultValue, Type controlType, Func<ISettings, bool> enabledCondition)
        {
            Name = name;
            DefaultValue = defaultValue;
            ControlType = controlType;
            EnabledCondition = enabledCondition;
        }

        public string Name { get; protected set; }
        public object DefaultValue { get; private set; }
        public Func<ISettings, bool> EnabledCondition { get; private set; }
        public Type ControlType { get; private set; }
    }
}
