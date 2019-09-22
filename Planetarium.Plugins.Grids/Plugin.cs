using Planetarium.Config;
using Planetarium.Types;
using Planetarium.Types.Config.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.Grids
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            AddSetting(new SettingItem("EquatorialGrid", false, "Grids"));
            AddSetting(new SettingItem("LabelEquatorialPoles", true, "Grids", s => s.Get<bool>("EquatorialGrid")));
            AddSetting(new SettingItem("HorizontalGrid", false, "Grids"));
            AddSetting(new SettingItem("LabelHorizontalPoles", true, "Grids", s => s.Get<bool>("HorizontalGrid")));
            AddSetting(new SettingItem("EclipticLine", true, "Grids"));
            AddSetting(new SettingItem("LabelEquinoxPoints", false, "Grids", s => s.Get<bool>("EclipticLine")));
            AddSetting(new SettingItem("LabelLunarNodes", false, "Grids", s => s.Get<bool>("EclipticLine")));
            AddSetting(new SettingItem("GalacticEquator", true, "Grids"));

            AddSetting(new SettingItem("EclipticColorNight", Color.FromArgb(0xC8, 0x80, 0x80, 0x00), "Colors"));
            AddSetting(new SettingItem("HorizontalGridColorNight", Color.FromArgb(0xC8, 0x00, 0x40, 0x00), "Colors"));

            AddToolbarItem(new ToolbarButton("Equatorial Grid", "IconEquatorialGrid", settings, "EquatorialGrid", "Grids"));
            AddToolbarItem(new ToolbarButton("Horizontal Grid", "IconHorizontalGrid", settings, "HorizontalGrid", "Grids"));
        }
    }
}
