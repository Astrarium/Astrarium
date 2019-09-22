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
            Add(new SettingItem("ConstLabels", true, "Constellations"));
            Add(new SettingItem("ConstLabelsType", ConstellationsRenderer.LabelType.InternationalName, "Constellations", s => s.Get<bool>("ConstLabels")));
            Add(new SettingItem("ConstLines", true, "Constellations"));
            Add(new SettingItem("ConstBorders", true, "Constellations"));

            Add(new SettingItem("Stars", true, "Stars"));
            Add(new SettingItem("StarsLabels", true, "Stars", s => s.Get<bool>("Stars")));
            Add(new SettingItem("StarsProperNames", true, "Stars", s => s.Get<bool>("Stars") && s.Get<bool>("StarsLabels")));

            // Default observer location.
            // Has no section, so not displayed in settings window.
            Add(new SettingItem("ObserverLocation", new CrdsGeographical(-44, 56.3333, +3, 80, "Europe/Moscow", "Nizhny Novgorod")));
        }
    }
}
