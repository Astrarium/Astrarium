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
using static ADK.Demo.Config.Settings;

namespace ADK.Demo.Config
{
    public class ConfigSectionHandler : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            string xml = section.OuterXml;
            XmlSerializer serializer = new XmlSerializer(typeof(SavedSettings), new XmlRootAttribute("SettingsDefaults"));
            StringReader stringReader = new StringReader(xml);
            return (SavedSettings)serializer.Deserialize(stringReader);
        }
    }
}
