using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Planetarium.Config
{
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
