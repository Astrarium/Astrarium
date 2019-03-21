using Planetarium.Config.ControlBuilders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Config
{
    public class SettingsConfig : ISettingsConfig
    {
        private readonly List<SettingConfigItem> Items = new List<SettingConfigItem>();
        private readonly Dictionary<Type, SettingControlBuilder> ControlBuilders = new Dictionary<Type, SettingControlBuilder>();

        public SettingsConfig()
        {
            ControlBuilders[typeof(bool)] = new BooleanSettingControlBuilder();
            ControlBuilders[typeof(string)] = new StringSettingControlBuilder();
            ControlBuilders[typeof(Enum)] = new EnumSettingControlBuilder();
            ControlBuilders[typeof(Color)] = new ColorSettingControlBuilder();
            ControlBuilders[typeof(Font)] = new FontSettingControlBuilder();
        }

        public SettingConfigItem Add<T>(string name, T defaultValue = default(T))
        {
            var item = new SettingConfigItem(name, typeof(T), defaultValue);
            Items.Add(item);
            return item;
        }

        public IEnumerator<SettingConfigItem> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public void RegisterControlBuilder(Type settingType, Type controlBuilderType)
        {
            ControlBuilders[settingType] = (SettingControlBuilder)Activator.CreateInstance(controlBuilderType.GetType());
        }

        public SettingControlBuilder GetBuilder(Type settingType)
        {
            var builder = ControlBuilders.ContainsKey(settingType) ? ControlBuilders[settingType] : null;

            if (builder == null)
            {
                builder = settingType.IsEnum && ControlBuilders.ContainsKey(typeof(Enum)) ? ControlBuilders[typeof(Enum)] : null;
            }

            return builder;
        }

        public SavedSettings GetDefaultSettings()
        {
            SavedSettings settings = new SavedSettings();
            Items.ForEach(i => settings.Settings.Add(new SavedSetting() {
                Name = i.Name,
                Type = i.Type.FullName,
                Value = TypeDescriptor.GetConverter(i.Type).ConvertToString(i.DefaultValue)
            }));
            return settings;
        }
    }
}
