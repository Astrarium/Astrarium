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

        /// <summary>
        /// Short type names dictionary
        /// </summary>
        private Dictionary<string, string> ShortTypeNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); 

        public Settings()
        {
            ShortTypeNames.Add("bool", typeof(bool).AssemblyQualifiedName);
            ShortTypeNames.Add("boolean", typeof(bool).AssemblyQualifiedName);
            ShortTypeNames.Add("color", typeof(System.Drawing.Color).AssemblyQualifiedName);
            ShortTypeNames.Add("int", typeof(int).AssemblyQualifiedName);
            ShortTypeNames.Add("integer", typeof(int).AssemblyQualifiedName);
            ShortTypeNames.Add("string", typeof(string).AssemblyQualifiedName);

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
            SettingsValues[settingName] = value;
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
                XmlSerializer serializer = new XmlSerializer(typeof(XmlSettings));
                var xml = (XmlSettings)serializer.Deserialize(reader);
                foreach (var s in xml.Settings)
                {
                    if (string.IsNullOrWhiteSpace(s.Name))
                    {
                        throw new FileFormatException($"Setting should have non-empty name.");
                    }

                    if (ShortTypeNames.ContainsKey(s.Type))
                    {
                        s.Type = ShortTypeNames[s.Type];
                    }

                    Type t = Type.GetType(s.Type, false, true);
                    if (t != null)
                    {
                        var converter = TypeDescriptor.GetConverter(t);
                        if (converter != null)
                        {
                            SettingsValues[s.Name] = converter.ConvertFromString(s.Value);
                        }
                        else
                        {

                            SettingsValues[s.Name] = Convert.ChangeType(s.Value, t);
                        }
                    }
                    else
                    {
                        throw new FileFormatException($"Setting {s.Name} has unknown type {s.Type}");
                    }
                }
            }
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        [Serializable]
        [XmlRoot("Settings")]
        public class XmlSettings
        {
            [XmlElement("Setting")]
            public XmlSettingNode[] Settings { get; set; }
        }

        [Serializable]
        [XmlType(AnonymousType = true, TypeName = "Setting")]
        public class XmlSettingNode
        {
            [XmlAttribute("name")]
            public string Name { get; set; }

            [XmlAttribute("type")]
            public string Type { get; set; } = "System.Boolean";

            [XmlAttribute("value")]
            public string Value { get; set; }
        }
    }    
}
