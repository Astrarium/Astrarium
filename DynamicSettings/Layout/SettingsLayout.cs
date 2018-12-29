using System;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace DynamicSettings.Layout
{
    [Serializable]
    [XmlRoot("SettingsLayout")]
    public class SettingsLayout
    {
        [XmlElement("Section")]
        public SettingsSection[] Sections { get; set; }

        public static SettingsLayout Load(Stream stream)
        {
            using (XmlReader reader = XmlReader.Create(stream))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SettingsLayout));
                var settingsTree = (SettingsLayout)serializer.Deserialize(reader);
                return settingsTree;
            }
        }
    }
}
