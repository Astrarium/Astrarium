using Astrarium.Algorithms;
using Astrarium.Config.Controls;
using Astrarium.Projections;
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
            SettingDefinitions.Add(new SettingDefinition("Language", "en", isPermanent: true));

            // App theme
            SettingDefinitions.Add(new SettingDefinition("AppTheme", "DeepBlue"));

            // Flag indicating main window should be maximized on startup
            SettingDefinitions.Add(new SettingDefinition("StartMaximized", false));

            // Check app updates on start
            SettingDefinitions.Add(new SettingDefinition("CheckUpdatesOnStart", true));

            // If set to true, window size will be remembered
            SettingDefinitions.Add(new SettingDefinition("RememberWindowSize", false));

            // Type of application menu
            SettingDefinitions.Add(new SettingDefinition("IsCompactMenu", false));

            // Toolbar visibility
            SettingDefinitions.Add(new SettingDefinition("IsToolbarVisible", true));

            // Status bar visibility
            SettingDefinitions.Add(new SettingDefinition("IsStatusBarVisible", true));

            // Default observer location
            SettingDefinitions.Add(new SettingDefinition("ObserverLocation", new CrdsGeographical(-44, 56.3333, +3, 80, "Nizhny Novgorod"), isPermanent: true));

            // Default tile server
            SettingDefinitions.Add(new SettingDefinition("ObserverLocationTileServer", "Offline map"));
            SettingDefinitions.Add(new SettingDefinition("ObserverLocationOverlayTileServer", ""));
            SettingDefinitions.Add(new SettingDefinition("ObserverLocationOverlayOpacity", 0.5f));

            // Default size of main window
            SettingDefinitions.Add(new SettingDefinition("WindowSize", System.Drawing.Size.Empty));

            // Projection
            SettingDefinitions.Add(new SettingDefinition("Projection", nameof(StereographicProjection)));

            // Night mode flag
            SettingDefinitions.Add(new SettingDefinition("NightMode", false));

            // Map transformation
            SettingDefinitions.Add(new SettingDefinition("FlipHorizontal", false));
            SettingDefinitions.Add(new SettingDefinition("FlipVertical", false));

            // View mode
            SettingDefinitions.Add(new SettingDefinition("ViewMode", ProjectionViewType.Horizontal));

            SettingSections.Add(new SettingSectionDefinition(typeof(GeneralSettingsSection), typeof(GeneralSettingsVM)));
            SettingSections.Add(new SettingSectionDefinition(typeof(ColorsSettingsSection), typeof(ColorsSettingsVM)));
            SettingSections.Add(new SettingSectionDefinition(typeof(FontsSettingsSection), typeof(FontsSettingsVM)));
            SettingSections.Add(new SettingSectionDefinition(typeof(RenderingOrderSettingsSection), typeof(RenderingOrderSettingsVM)));
        }
    }
}
