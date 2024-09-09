using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Astrarium.ViewModels
{
    public class AboutVM : ViewModelBase
    {
        public string ProductName => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName;
        public string ProductDescription => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).Comments;
        public string Version => Text.Get("AboutWindow.Version", ("version", FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion));
        public string Copyright => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).LegalCopyright;

        public string Credits
        {
            get
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Astrarium.Credits.md"))
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public IEnumerable<PluginInfo> Plugins 
        {
            get 
            {
                var pluginTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => typeof(AbstractPlugin).IsAssignableFrom(t) && !t.IsAbstract);

                foreach (var pluginType in pluginTypes)
                {
                    string location = Assembly.GetAssembly(pluginType).Location;
                    var versionInfo = FileVersionInfo.GetVersionInfo(location);
                    yield return new PluginInfo()
                    {
                        Name = versionInfo.ProductName,
                        Version = versionInfo.ProductVersion,
                        Description = versionInfo.Comments,
                        Authors = versionInfo.CompanyName,
                    };
                }               
            }
        }

        public class PluginInfo
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Authors { get; set; }
            public string Version { get; set; }
        }
    }
}
