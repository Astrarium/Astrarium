using Planetarium.Config;
using Planetarium.Types.Config.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Planetarium.Types
{
    public abstract class AbstractPlugin
    {
        public ICollection<SettingItem> SettingItems { get; } = new List<SettingItem>();
        public ICollection<ToolbarButton> ToolbarItems { get; } = new List<ToolbarButton>();

        protected void AddSetting(SettingItem setting)
        {
            SettingItems.Add(setting);
        }

        protected void AddToolbarItem(ToolbarButton button)
        {
            ToolbarItems.Add(button);
        }

        protected void ExportResourceDictionaries(params string[] names)
        {
            string assemblyName = Assembly.GetAssembly(GetType()).FullName;
            foreach (string name in names)
            {
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri($"/{assemblyName};component/{name}", UriKind.Relative) });
            }
        }
    }
}
