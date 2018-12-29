using System;
using System.Xml.Serialization;

namespace DynamicSettings.Layout
{
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
}
