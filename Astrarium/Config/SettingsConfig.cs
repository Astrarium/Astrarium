using Astrarium.Algorithms;
using Astrarium.Types;
using System.Collections.Generic;
using System.Drawing;

namespace Astrarium.Config
{
    public class SettingsConfig : List<SettingItem>
    {
        /// <summary>
        /// Default settings going here
        /// </summary>
        public SettingsConfig()
        {
            // Default language
            Add(new SettingItem("Language", "en", "UI", typeof(LanguageSettingControl)));
            
            // Type of application menu
            Add(new SettingItem("IsCompactMenu", false, "UI"));

            // Toolbar visibility
            Add(new SettingItem("IsToolbarVisible", true, "UI"));

            // Status bar visibility
            Add(new SettingItem("IsStatusBarVisible", true, "UI"));

            // Default observer location.
            // Has no section, so not displayed in settings window.
            Add(new SettingItem("ObserverLocation", new CrdsGeographical(-44, 56.3333, +3, 80, "Europe/Moscow", "Nizhny Novgorod")));

            // Default color schema
            Add(new SettingItem("Schema", ColorSchema.Night, "Colors"));
        }
    }
}
