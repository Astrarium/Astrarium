using Astrarium.Algorithms;
using Astrarium.Config.Controls;
using Astrarium.Types;
using Astrarium.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium
{
    public class UIElementsIntegration
    {
        public UIElementsConfig<string, ToolbarButtonBase> ToolbarButtons { get; } = new UIElementsConfig<string, ToolbarButtonBase>();
        public UIElementsConfig<MenuItemPosition, MenuItem> MenuItems { get; } = new UIElementsConfig<MenuItemPosition, MenuItem>();
        public List<SettingDefinition> SettingDefinitions { get; } = new List<SettingDefinition>();
        public List<SettingSectionDefinition> SettingSections { get; } = new List<SettingSectionDefinition>();

        public UIElementsIntegration()
        {
            // Default language
            SettingDefinitions.Add(new SettingDefinition("Language", "en", false));

            // Flag indicating main window should be maximized on startup
            SettingDefinitions.Add(new SettingDefinition("StartMaximized", false, false));
            
            // If set to true, window size will be remembered
            SettingDefinitions.Add(new SettingDefinition("RememberWindowSize", false, false));

            // Type of application menu
            SettingDefinitions.Add(new SettingDefinition("IsCompactMenu", false, false));

            // Toolbar visibility
            SettingDefinitions.Add(new SettingDefinition("IsToolbarVisible", true, false));

            // Status bar visibility
            SettingDefinitions.Add(new SettingDefinition("IsStatusBarVisible", true, false));

            // Default observer location
            SettingDefinitions.Add(new SettingDefinition("ObserverLocation", new CrdsGeographical(-44, 56.3333, +3, 80, "Europe/Moscow", "Nizhny Novgorod"), true));

            // Default size of main window
            SettingDefinitions.Add(new SettingDefinition("WindowSize", System.Drawing.Size.Empty, false));

            // Default color schema
            SettingDefinitions.Add(new SettingDefinition("Schema", ColorSchema.Night, false));

            // Map transformation
            SettingDefinitions.Add(new SettingDefinition("IsMirrored", false, false));
            SettingDefinitions.Add(new SettingDefinition("IsInverted", false, false));

            SettingSections.Add(new SettingSectionDefinition(typeof(GeneralSettingsSection), typeof(GeneralSettingsVM)));
            SettingSections.Add(new SettingSectionDefinition(typeof(ColorsSettingsSection), typeof(ColorsSettingsVM)));
            SettingSections.Add(new SettingSectionDefinition(typeof(FontsSettingsSection), typeof(FontsSettingsVM)));
            SettingSections.Add(new SettingSectionDefinition(typeof(RenderingOrderSettingsSection), typeof(RenderingOrderSettingsVM)));
        }
    }
}
