using Astrarium.Types.Controls;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Astrarium.Types
{
    /// <summary>
    /// Base class for all Astrarium plugins
    /// </summary>
    public abstract class AbstractPlugin : PropertyChangedBase
    {
        /// <summary>
        /// Gets list of settings definitions
        /// </summary>
        public List<SettingDefinition> SettingDefinitions { get; } = new List<SettingDefinition>();

        /// <summary>
        /// Gets list of settings sections definitions
        /// </summary>
        public List<SettingSectionDefinition> SettingSections { get; } = new List<SettingSectionDefinition>();

        /// <summary>
        /// Gets configurations of toolbar items
        /// </summary>
        public UIElementsConfig<string, ToolbarButtonBase> ToolbarItems { get; } = new UIElementsConfig<string, ToolbarButtonBase>();

        /// <summary>
        /// Gets configurations of menu items
        /// </summary>
        public UIElementsConfig<MenuItemPosition, MenuItem> MenuItems { get; } = new UIElementsConfig<MenuItemPosition, MenuItem>();

        /// <summary>
        /// Gets collection of extensions for Object Info Window.
        /// </summary>
        public List<ObjectInfoExtension> ObjectInfoExtensions { get; } = new List<ObjectInfoExtension>();

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

        protected void ExtendObjectInfo(string title, Func<CelestialObject, FrameworkElement> viewProvider)
        {
            ObjectInfoExtensions.Add(new ObjectInfoExtension(title, viewProvider));
        }

        /// <summary>
        /// Defines a setting with specified name and default value.
        /// </summary>
        /// <param name="name">Unique setting name.</param>
        /// <param name="defaultValue">Default setting name.</param>
        /// <param name="isPermanent">Flag indicating the setting should not be resetted.</param>
        protected void DefineSetting(string name, object defaultValue, bool isPermanent = false)
        {
            SettingDefinitions.Add(new SettingDefinition(name, defaultValue, isPermanent));
        }

        /// <summary>
        /// Defines UI section in settings window.
        /// </summary>
        /// <typeparam name="TSectionControl">Type of UI control responsive for displaying settings.</typeparam>
        /// <typeparam name="TViewModel">Type of ViewModel for the UI control.</typeparam>
        protected void DefineSettingsSection<TSectionControl, TViewModel>() where TSectionControl : SettingsSection where TViewModel : ViewModelBase
        {
            SettingSections.Add(new SettingSectionDefinition(typeof(TSectionControl), typeof(TViewModel)));
        }

        /// <summary>
        /// Called when the plugin is ready to be initialized
        /// </summary>
        public virtual void Initialize() { }
    }
}
