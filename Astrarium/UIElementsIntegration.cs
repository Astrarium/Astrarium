using Astrarium.Algorithms;
using Astrarium.Config.Controls;
using Astrarium.Types;
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
        public UIElementsConfig<string, SettingItem> SettingItems { get; } = new UIElementsConfig<string, SettingItem>();
        
        public UIElementsIntegration()
        {
            // Default language
            SettingItems.Add("General", new SettingItem("Language", CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower(), typeof(LanguageSettingControl)));

            // Flag indicating the app should be started with maximized main window
            SettingItems.Add("General", new SettingItem("StartMaximized", false));

            SettingItems.Add("General", new SettingItem("RememberWindowSize", false, s => !s.Get("StartMaximized")));

            // Type of application menu
            SettingItems.Add("General", new SettingItem("IsCompactMenu", false));

            // Toolbar visibility
            SettingItems.Add("General", new SettingItem("IsToolbarVisible", true));

            // Status bar visibility
            SettingItems.Add("General", new SettingItem("IsStatusBarVisible", true));

            // Default azumuth measurement origin
            SettingItems.Add("General", new SettingItem("AzimuthOrigin", AzimuthOrigin.South));

            // Default observer location.
            // Has no section, so not displayed in settings window.
            SettingItems.Add(null, new SettingItem("ObserverLocation", new CrdsGeographical(-44, 56.3333, +3, 80, "Europe/Moscow", "Nizhny Novgorod")));

            // Default size of main window
            SettingItems.Add(null, new SettingItem("WindowSize", System.Drawing.Size.Empty));

            // Default color schema
            SettingItems.Add("Colors", new SettingItem("Schema", ColorSchema.Night));
        }
    }
}
