using Astrarium.Algorithms;
using Astrarium.Config;
using Astrarium.Types;
using System;
using System.Collections.Generic;
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
            SettingItems.Add("UI", new SettingItem("Language", "en", "UI", typeof(LanguageSettingControl)));

            // Type of application menu
            SettingItems.Add("UI", new SettingItem("IsCompactMenu", false, "UI"));

            // Toolbar visibility
            SettingItems.Add("UI", new SettingItem("IsToolbarVisible", true, "UI"));

            // Status bar visibility
            SettingItems.Add("UI", new SettingItem("IsStatusBarVisible", true, "UI"));

            // Default observer location.
            // Has no section, so not displayed in settings window.
            SettingItems.Add(null, new SettingItem("ObserverLocation", new CrdsGeographical(-44, 56.3333, +3, 80, "Europe/Moscow", "Nizhny Novgorod")));

            // Default color schema
            SettingItems.Add("Colors", new SettingItem("Schema", ColorSchema.Night, "Colors"));
        }
    }
}
