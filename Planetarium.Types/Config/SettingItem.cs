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
        public SettingItem(string name, object defaultValue) :
            this(name, defaultValue, null, null, null) { }

        public SettingItem(string name, object defaultValue, string sectionName) :
            this (name, defaultValue, sectionName, null, null) { }

        public SettingItem(string name, object defaultValue, string sectionName, Type controlType) : 
            this(name, defaultValue, sectionName, controlType, null) { }

        public SettingItem(string name, object defaultValue, string sectionName, Func<ISettings, bool> enabledCondition) :
            this(name, defaultValue, sectionName, null, enabledCondition) { }

        public SettingItem(string name, object defaultValue, string sectionName, Type controlType, Func<ISettings, bool> enabledCondition)
        {
            Name = name;
            DefaultValue = defaultValue;
            Section = sectionName;
            ControlType = controlType;
            EnabledCondition = enabledCondition;
        }

        public string Name { get; protected set; }
        public object DefaultValue { get; private set; }
        public string Section { get; private set; }
        public Func<ISettings, bool> EnabledCondition { get; private set; }
        public Type ControlType { get; private set; }
    }
}
