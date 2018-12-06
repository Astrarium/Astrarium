using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ADK.Demo
{
    /// <summary>
    /// Defines methods to work with application settings
    /// </summary>
    public interface ISettings
    {
        /// <summary>
        /// Gets setting with specified name and casts its value to desired type.
        /// </summary>
        /// <typeparam name="T">Type of setting value</typeparam>
        /// <param name="settingName">Unique name of setting</param>
        /// <returns>Setting value, of defailt value for type <typeparamref name="T"/>.</returns>
        T Get<T>(string settingName);

        /// <summary>
        /// Sets value of setting with specified name
        /// </summary>
        /// <param name="settingName"></param>
        /// <param name="value"></param>
        void Set(string settingName, object value);

        /// <summary>
        /// Loads settings
        /// </summary>
        void Load();

        /// <summary>
        /// Saves settings
        /// </summary>
        void Save();

        /// <summary>
        /// Gets settings structure
        /// </summary>
        SettingsTree Tree { get; }

        /// <summary>
        /// Gets all settings
        /// </summary>
        ICollection<SettingNode> All { get; }

        /// <summary>
        /// Fired when new setting value is set via calling method <see cref="Set(string, object)" />.
        /// </summary>
        event Action<string> OnSettingValueChanged;

        /// <summary>
        /// Flag indicating at least one setting has been changed via calling method <see cref="Set(string, object)" />.
        /// </summary>
        bool IsChanged { get; }
    }

    [Serializable]
    [XmlRoot("Settings")]
    public class SettingsTree
    {
        [XmlElement("Section")]
        public SettingsSection[] Sections { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, TypeName = "Section")]
    public class SettingsSection
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("title")]
        public string Title { get; set; }

        [XmlElement("Setting")]
        public SettingNode[] Settings { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, TypeName = "Setting")]
    public class SettingNode
    {
        /// <summary>
        /// Short type names dictionary
        /// </summary>
        private static Dictionary<string, Type> ShortTypeNames = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Default control types
        /// </summary>
        private static Dictionary<Type, string> DefaultControlTypes = new Dictionary<Type, string>();

        /// <summary>
        /// Static initializer
        /// </summary>
        static SettingNode()
        {
            ShortTypeNames.Add("bool", typeof(bool));
            ShortTypeNames.Add("boolean", typeof(bool));
            ShortTypeNames.Add("color", typeof(System.Drawing.Color));
            ShortTypeNames.Add("int", typeof(int));
            ShortTypeNames.Add("integer", typeof(int));
            ShortTypeNames.Add("string", typeof(string));

            DefaultControlTypes.Add(typeof(bool), "checkbox");
            DefaultControlTypes.Add(typeof(string), "textbox");
            DefaultControlTypes.Add(typeof(System.Drawing.Color), "colorpicker");
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; } = "System.Boolean";

        [XmlAttribute("value")]
        public string Value { get; set; }

        [XmlAttribute("title")]
        public string Title { get; set; }

        [XmlAttribute("control")]
        public string Control { get; set; }

        [XmlAttribute("dependsOn")]
        public string DependsOn { get; set; }

        [XmlAttribute("enabledIf")]
        public string EnabledIf { get; set; }

        [XmlAttribute("visibleIf")]
        public string VisibleIf { get; set; }

        public Type ValueType
        {
            get
            {
                Type t = typeof(string);
                if (ShortTypeNames.ContainsKey(Type))
                {
                    t = ShortTypeNames[Type];
                }
                else
                {
                    t = System.Type.GetType(Type, false, true);
                }
                return t;
            }
        }

        public object ValueFromString(string value)
        {
            Type t = ValueType;
            var converter = TypeDescriptor.GetConverter(t);
            if (converter != null)
            {
                return converter.ConvertFromString(value);
            }
            else
            {
                return Convert.ChangeType(value, t);
            }
        }

        public string ValueToString(object value)
        {
            Type t = ValueType;           
            var converter = TypeDescriptor.GetConverter(t);
            if (converter != null)
            {
                return converter.ConvertToString(value);
            }
            else
            {
                return value.ToString();
            }            
        }

        public string DefaultControl
        {
            get
            {
                Type t = ValueType;
                if (DefaultControlTypes.ContainsKey(ValueType))
                {
                    return DefaultControlTypes[t];
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
