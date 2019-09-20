using Planetarium.Config;
using Planetarium.Renderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Planetarium.Types
{
    public abstract class AbstractPlugin
    {
        private List<SettingItem> settingItems = new List<SettingItem>();
        private List<ToolbarButton> toolbarItems = new List<ToolbarButton>();

        public IEnumerable<SettingItem> SettingItems => settingItems;
        public IEnumerable<ToolbarButton> ToolbarItems => toolbarItems;

        public IEnumerable<Type> Renderers
        {
            get
            {
                return GetType().Assembly.GetTypes().Where(t => typeof(BaseRenderer).IsAssignableFrom(t) && !t.IsAbstract);
            }
        }

        public IEnumerable<Type> Calculators
        {
            get
            {
                return GetType().Assembly.GetTypes().Where(t => typeof(BaseCalc).IsAssignableFrom(t) && !t.IsAbstract);
            }
        }

        public IEnumerable<Type> AstroEventProviders
        {
            get
            {
                return GetType().Assembly.GetTypes().Where(t => typeof(BaseAstroEventsProvider).IsAssignableFrom(t) && !t.IsAbstract);
            }
        }

        protected void AddSetting(SettingItem setting)
        {
            settingItems.Add(setting);
        }

        protected void AddToolbarItem(ToolbarButton button)
        {
            toolbarItems.Add(button);
        }

        protected void ExportResourceDictionaries(params string[] names)
        {
            string assemblyName = GetType().Assembly.FullName;
            foreach (string name in names)
            {
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri($"/{assemblyName};component/{name}", UriKind.Relative) });
            }
        }
    }
}
