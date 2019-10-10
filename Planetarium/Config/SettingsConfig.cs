using ADK;
using Planetarium.Renderers;
using Planetarium.Types;
using Planetarium.Types.Config.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Config
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

            // Default observer location.
            // Has no section, so not displayed in settings window.
            Add(new SettingItem("ObserverLocation", new CrdsGeographical(-44, 56.3333, +3, 80, "Europe/Moscow", "Nizhny Novgorod")));

            // Default color schema
            Add(new SettingItem("Schema", ColorSchema.Night, "Colors"));

            // Sky colors
            Add(new SettingItem("ColorSky", new SkyColor() { Night = Color.Black, Day = Color.FromArgb(116, 184, 254), Red = Color.Black, White = Color.White }));
        }
    }
}
