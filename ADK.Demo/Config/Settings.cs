using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ADK.Demo.Config
{
    public class Settings : DynamicObject, ISettings, INotifyPropertyChanged
    {
        private readonly string SETTINGS_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ADK", "Settings.xml");

        /// <summary>
        /// Contains settings values
        /// </summary>
        private Dictionary<string, object> SettingsValues = new Dictionary<string, object>();

        /// <summary>
        /// Short type names dictionary
        /// </summary>
        private static Dictionary<string, Type> ShortTypeNames = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        public event Action<string, object> SettingValueChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsChanged { get; private set; }

        static Settings()
        {
            ShortTypeNames.Add("bool", typeof(bool));
            ShortTypeNames.Add("boolean", typeof(bool));
            ShortTypeNames.Add("font", typeof(Font));
            ShortTypeNames.Add("color", typeof(Color));
            ShortTypeNames.Add("int", typeof(int));
            ShortTypeNames.Add("integer", typeof(int));
            ShortTypeNames.Add("string", typeof(string));
            ShortTypeNames.Add("text", typeof(string));
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
            if (SettingsValues.ContainsKey(settingName) )
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
            Load(ConfigurationManager.GetSection("SettingsDefaults") as SavedSettings);
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
            using (XmlReader reader = XmlReader.Create(stream))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SavedSettings));
                var settingsTree = (SavedSettings)serializer.Deserialize(reader);
                Load(settingsTree);
            }
        }

        private void Load(SavedSettings settingsTree)
        {
            foreach (var setting in settingsTree.Settings)
            {
                var type = ShortTypeNames.ContainsKey(setting.Type) ? ShortTypeNames[setting.Type] : Type.GetType(setting.Type, false, true);

                if (type != null)
                {
                    var converter = TypeDescriptor.GetConverter(type);

                    Set(setting.Name, converter.ConvertFromString(setting.Value));
                }
                else
                {
                    throw new FileFormatException($"Setting `{setting.Name}` has unknown type `{setting.Type}`.");
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
        }

        private void Save(Stream stream)
        {
            using (XmlWriter writer = XmlWriter.Create(stream))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SavedSettings));
                SavedSettings saved = new SavedSettings();
                foreach (var s in SettingsValues)
                {
                    var converter = TypeDescriptor.GetConverter(s.Value.GetType());
                    string stringValue = converter != null ? converter.ConvertToString(s.Value) : s.Value.ToString();
                    saved.Settings.Add(new SavedSetting() { Name = s.Key, Value = stringValue, Type = s.Value.GetType().AssemblyQualifiedName });
                }

                serializer.Serialize(writer, saved);
                IsChanged = false;
            }
        }

        [Serializable]
        [XmlRoot("Settings")]
        public class SavedSettings
        {
            [XmlElement("Setting")]
            public List<SavedSetting> Settings { get; set; } = new List<SavedSetting>();
        }

        [Serializable]
        [XmlType(AnonymousType = true, TypeName = "Setting")]
        public class SavedSetting
        { 
            [XmlAttribute("name")]
            public string Name { get; set; }

            [XmlAttribute("type")]
            public string Type { get; set; }

            [XmlAttribute("value")]
            public string Value { get; set; }
        }
    }    
}
