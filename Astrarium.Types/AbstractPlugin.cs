using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Astrarium.Types
{
    /// <summary>
    /// Base class for all Astrarium plugins
    /// </summary>
    public abstract class AbstractPlugin : PropertyChangedBase
    {
        /// <summary>
        /// Gets configurations of settings items
        /// </summary>
        public UIElementsConfig<string, SettingItem> SettingItems { get; } = new UIElementsConfig<string, SettingItem>();

        /// <summary>
        /// Gets configurations of toolbar items
        /// </summary>
        public UIElementsConfig<string, ToolbarButtonBase> ToolbarItems { get; } = new UIElementsConfig<string, ToolbarButtonBase>();

        /// <summary>
        /// Gets configurations of menu items
        /// </summary>
        public UIElementsConfig<MenuItemPosition, MenuItem> MenuItems { get; } = new UIElementsConfig<MenuItemPosition, MenuItem>();
        
        /// <summary>
        /// Exports resource dictionaries to the application
        /// </summary>
        /// <param name="names">Resource dictionaries' names to be exported</param>
        protected void ExportResourceDictionaries(params string[] names)
        {
            string assemblyName = GetType().Assembly.FullName;
            foreach (string name in names)
            {
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri($"/{assemblyName};component/{name}", UriKind.Relative) });
            }
        }

        /// <summary>
        /// Called when the plugin is ready to be initialized
        /// </summary>
        public virtual void Initialize() { }
    }
}
