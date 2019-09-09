using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;

namespace Planetarium.Config
{
    public class Settings : DynamicObject, ISettings, INotifyPropertyChanged
    {
        private readonly string SETTINGS_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ADK", "Settings.json");

        /// <summary>
        /// Contains settings values
        /// </summary>
        private Dictionary<string, object> SettingsValues = new Dictionary<string, object>();

        private SavedSettings Defaults = new SavedSettings();

        public event Action<string, object> SettingValueChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsChanged { get; private set; }

        public void SetDefaults(SavedSettings defaults)
        {
            Defaults = defaults;
        }

        public T Get<T>(string settingName, T defaultValue = default(T))
        {
            if (SettingsValues.ContainsKey(settingName))
            {
                return (T)SettingsValues[settingName];
            }
            else
            {
                return defaultValue;
            }
        }

        public void Set(string settingName, object value)
        {
            if (SettingsValues.ContainsKey(settingName))
            {
                var oldValue = SettingsValues[settingName];
                if (!oldValue.Equals(value))
                {
                    SettingsValues[settingName] = value;
                    IsChanged = true;
                    SettingValueChanged?.Invoke(settingName, value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(settingName));
                }
            }
            else
            {
                SettingsValues[settingName] = value;
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string name = binder.Name;
            if (!SettingsValues.TryGetValue(name, out result))
            {
                result = name;
            }
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Set(binder.Name, value);
            return true;
        }

        /// <summary>
        /// Loads default settings
        /// </summary>
        public void Reset()
        {
            Load(Defaults);
        }

        public void Load()
        {
            Reset();
            if (File.Exists(SETTINGS_PATH))
            {
                using (var stream = new FileStream(SETTINGS_PATH, FileMode.Open))
                {
                    Load(stream);
                }
            }
        }

        private void Load(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            using (JsonTextReader jsonReader = new JsonTextReader(reader))
            {
                JsonSerializer ser = new JsonSerializer();
                var settingsTree = ser.Deserialize<SavedSettings>(jsonReader);
                Load(settingsTree);
            }
        }

        private void Load(SavedSettings settingsTree)
        {
            foreach (var setting in settingsTree)
            {
                var type = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.Equals(setting.Value.GetType()));

                if (type != null)
                {
                    Set(setting.Name, setting.Value);
                }
                else
                {
                    throw new FileFormatException($"Setting `{setting.Name}` has unknown type `{setting.Value.GetType()}`.");
                }
            }
            IsChanged = false;
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SETTINGS_PATH));
            using (var stream = new FileStream(SETTINGS_PATH, FileMode.Create))
            {
                Save(stream);
            }
            IsChanged = false;
        }

        private void Save(Stream stream)
        {
            using (StreamWriter writer = new StreamWriter(stream))
            using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
            {
                SavedSettings saved = new SavedSettings();
                foreach (var s in SettingsValues)
                {
                    saved.Add(new SavedSetting() { Name = s.Key, Value = s.Value });
                }

                JsonSerializer ser = new JsonSerializer() { Formatting = Formatting.Indented, ContractResolver = new WritablePropertiesOnlyResolver() };
                ser.Serialize(jsonWriter, saved);
                jsonWriter.Flush();
            }
        }

        private class WritablePropertiesOnlyResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> props = base.CreateProperties(type, memberSerialization);
                return props.Where(p => p.Writable).ToList();
            }
        }
    }
}
