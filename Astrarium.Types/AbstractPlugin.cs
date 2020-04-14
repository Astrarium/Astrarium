using Astrarium.Config;
using Astrarium.Renderers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Astrarium.Types
{
    public abstract class AbstractPlugin : PropertyChangedBase
    {
        public UIElementsConfig<string, SettingItem> SettingItems { get; } = new UIElementsConfig<string, SettingItem>();
        public UIElementsConfig<string, ToolbarButtonBase> ToolbarItems { get; } = new UIElementsConfig<string, ToolbarButtonBase>();
        public UIElementsConfig<MenuItemPosition, MenuItem> MenuItems { get; } = new UIElementsConfig<MenuItemPosition, MenuItem>();

        public static IEnumerable<Type> Renderers(Type pluginType)
        {            
            return pluginType.Assembly.GetTypes().Where(t => typeof(BaseRenderer).IsAssignableFrom(t) && !t.IsAbstract);
        }

        public static IEnumerable<Type> Calculators(Type pluginType)
        {
            return pluginType.Assembly.GetTypes().Where(t => typeof(BaseCalc).IsAssignableFrom(t) && !t.IsAbstract);
        }

        public static IEnumerable<Type> AstroEventProviders(Type pluginType)
        {
            return pluginType.Assembly.GetTypes().Where(t => typeof(BaseAstroEventsProvider).IsAssignableFrom(t) && !t.IsAbstract);
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
