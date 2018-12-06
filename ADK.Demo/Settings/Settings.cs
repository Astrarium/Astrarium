using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ADK.Demo.Settings
{
    public class Settings : ISettings
    {
        /// <summary>
        /// Contains settings values
        /// </summary>
        private Dictionary<string, object> SettingsValues = new Dictionary<string, object>();

        /// <summary>
        /// Path to store settings
        /// </summary>
        private string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ADK", "Settings.xml");

        public SettingsTree Tree { get; private set; }

        public ICollection<SettingNode> All { get; } = new List<SettingNode>();

        public bool IsChanged { get; private set; }

        public event Action<string> OnSettingValueChanged;

        public Settings()
        {
            // Initialize with default values
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ADK.Demo.Settings.Settings.xml"))
            {
                Load(stream);
            }
        }

        public T Get<T>(string settingName)
        {
            return (T)SettingsValues[settingName];
        }

        public void Set(string settingName, object value)
        {
            if (SettingsValues.ContainsKey(settingName) )
            {
                var oldValue = SettingsValues[settingName];

                if (!oldValue.Equals(value))
                {
                    SettingsValues[settingName] = value;
                    OnSettingValueChanged?.Invoke(settingName);
                    IsChanged = true;
                }
            }
        }

        public void Load()
        {
            if (File.Exists(SettingsPath))
            {
                using (var stream = new FileStream(SettingsPath, FileMode.Open))
                {
                    Load(stream);
                }
            }
        }

        private void Load(Stream stream)
        {
            using (XmlReader reader = XmlReader.Create(stream))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SettingsTree));
                Tree = (SettingsTree)serializer.Deserialize(reader);

                foreach (var section in Tree.Sections)
                {
                    if (string.IsNullOrWhiteSpace(section.Name))
                    {
                        throw new FileFormatException($"Section should have non-empty name.");
                    }

                    foreach (var setting in section.Settings)
                    {
                        if (string.IsNullOrWhiteSpace(setting.Name))
                        {
                            throw new FileFormatException($"Setting should have non-empty name.");
                        }

                        if (setting.ValueType == null)
                        {
                            throw new FileFormatException($"Setting `{setting.Name}` has unknown type `{setting.Type}`.");
                        }

                        //if (setting.DefaultControl == null)
                        //{
                        //   throw new FileFormatException($"Setting `{setting.Name}` has unknown control type `{setting.Control}`.");
                        //}

                        SettingsValues[setting.Name] = setting.ValueFromString(setting.Value);
                        All.Add(setting);
                    }
                }

                var settings = Tree.Sections.SelectMany(s => s.Settings);
                var dependentSettings = settings.Where(s => !string.IsNullOrEmpty(s.DependsOn));
                foreach (var setting in dependentSettings)
                {
                    if (settings.All(s => s.Name != setting.DependsOn))
                    {
                        throw new FileFormatException($"Setting `{setting.Name}` depends on undefined setting `{setting.DependsOn}`.");
                    }

                    if (string.IsNullOrEmpty(setting.EnabledIf) && string.IsNullOrEmpty(setting.VisibleIf))
                    {
                        throw new FileFormatException($"Setting `{setting.Name}` depends on setting `{setting.DependsOn}` but neither `enabledIf` or `visibleIf` attributes were specified.");
                    } 
                }
            }
        }

        public void Save()
        {
            string directory = Path.GetDirectoryName(SettingsPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            using (var stream = new FileStream(SettingsPath, FileMode.OpenOrCreate))
            {
                Save(stream);
            }
        }

        private void Save(Stream stream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SettingsTree));

            foreach (var section in Tree.Sections)
            {
                foreach (var setting in section.Settings)
                {
                    setting.Value = setting.ValueToString(SettingsValues[setting.Name]);
                }    
            }

            serializer.Serialize(stream, Tree);           
        }
    }    
}
