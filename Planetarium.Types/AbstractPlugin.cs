using Planetarium.Config;
using Planetarium.Types.Config.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Types
{
    public class AbstractSettingsConfig
    {
        public ICollection<SettingItem> SettingItems { get; } = new List<SettingItem>();

        protected void AddSetting<T>(string name, T defaultValue)
        {
            AddSetting(name, defaultValue, null, null, null);
        }

        protected void AddSetting<T>(string name, T defaultValue, string sectionName)
        {
            AddSetting(name, defaultValue, sectionName, null, null);
        }

        protected void AddSetting<T>(string name, T defaultValue, string sectionName, Type controlType)
        {
            AddSetting(name, defaultValue, sectionName, controlType, null);
        }

        protected void AddSetting<T>(string name, T defaultValue, string sectionName, Func<ISettings, bool> enabledCondition)
        {
            AddSetting(name, defaultValue, sectionName, null, enabledCondition);
        }

        protected void AddSetting<T>(string name, T defaultValue, string sectionName, Type controlType, Func<ISettings, bool> enabledCondition)
        {
            SettingItems.Add(new SettingItem()
            {
                Name = name,
                DefaultValue = defaultValue,
                Section = sectionName,
                ControlType = controlType,
                EnabledCondition = enabledCondition
            });
        }
    }

    public abstract class AbstractPlugin : AbstractSettingsConfig
    {
        
    }
}
