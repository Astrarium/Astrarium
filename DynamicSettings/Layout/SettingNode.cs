using System;
using System.Xml.Serialization;

namespace DynamicSettings.Layout
{
    [Serializable]
    [XmlType(AnonymousType = true, TypeName = "Setting")]
    public class SettingNode
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("title")]
        public string Title { get; set; }

        [XmlAttribute("editor")]
        public string Editor { get; set; }

        [XmlAttribute("enabledIf")]
        public string EnabledIf { get; set; }

        [XmlAttribute("visibleIf")]
        public string VisibleIf { get; set; }
    }
}
