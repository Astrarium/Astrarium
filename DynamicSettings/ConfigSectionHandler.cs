using DynamicSettings.Layout;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DynamicSettings
{
    public class ConfigSectionHandler : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            string xml = section.OuterXml;
            XmlSerializer serializer = new XmlSerializer(typeof(SettingsLayout), new XmlRootAttribute("SettingsLayout"));
            StringReader stringReader = new StringReader(xml);
            return (SettingsLayout)serializer.Deserialize(stringReader);
        }
    }
}
