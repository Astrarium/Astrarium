using Planetarium.Config;
using Planetarium.Renderers;
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
            SettingItems.Add(setting);
        }

        protected void AddToolbarItem(ToolbarButton button)
        {
            ToolbarItems.Add(button);
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
