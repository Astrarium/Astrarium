using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Planetarium.Config.ControlBuilders;

namespace Planetarium.Config
{
    public class SettingConfigItem
    {
        public string Name { get; private set; }
        public string Section { get; private set; }
        public Type Type { get; private set; }
        public object DefaultValue { get; private set; }
        public Func<ISettings, bool> EnabledWhenCondition { get; private set; }
        public Func<ISettings, bool> VisibleWhenCondition { get; private set; }
        public SettingControlBuilder Builder { get; private set; }

        public SettingConfigItem(string name, Type type, object defaultValue)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
        }

        public SettingConfigItem EnabledWhen(Func<ISettings, bool> condition)
        {
            EnabledWhenCondition = condition;
            return this;
        }

        public SettingConfigItem EnabledWhenTrue(string settingName)
        {
            EnabledWhenCondition = (s) => s.Get<bool>(settingName);
            return this;
        }

        public SettingConfigItem EnabledWhenFalse(string settingName)
        {
            EnabledWhenCondition = (s) => !s.Get<bool>(settingName);
            return this;
        }

        public SettingConfigItem VisibleWhen(Func<ISettings, bool> condition)
        {
            VisibleWhenCondition = condition;
            return this;
        }

        public SettingConfigItem VisibleWhenTrue(string settingName)
        {
            VisibleWhenCondition = (s) => s.Get<bool>(settingName);
            return this;
        }

        public SettingConfigItem VisibleWhenFalse(string settingName)
        {
            VisibleWhenCondition = (s) => !s.Get<bool>(settingName);
            return this;
        }

        public SettingConfigItem WithBuilder(Type builderType)
        {
            if (!typeof(SettingControlBuilder).IsAssignableFrom(builderType))
                throw new ArgumentException($"Builder type should be derived from {nameof(SettingControlBuilder)} base type.");

            if (builderType.IsAbstract)
                throw new ArgumentException($"Builder type should not be an abstract class type.");

            if (builderType.GetConstructor(Type.EmptyTypes) == null)
                throw new ArgumentException($"Builder type should have public parameterless constructor.");

            Builder = (SettingControlBuilder)Activator.CreateInstance(builderType);

            return this;
        }

        public SettingConfigItem WithSection(string sectionName)
        {
            Section = sectionName;
            return this;
        }
    }
}
