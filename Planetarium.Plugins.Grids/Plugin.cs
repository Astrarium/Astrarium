using Planetarium.Config;
using Planetarium.Types;
using Planetarium.Types.Config.Controls;
using Planetarium.Types.Localization;
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

            AddSetting(new SettingItem("ColorEcliptic", new SkyColor() { Night = Color.FromArgb(0xC8, 0x80, 0x80, 0x00), Day = Color.FromArgb(255, 255, 128), White = Color.FromArgb(150, 150, 150) }));
            AddSetting(new SettingItem("HorizontalGridColorNight", Color.FromArgb(0xC8, 0x00, 0x40, 0x00), "Colors"));

            AddToolbarItem(new ToolbarButton(Text.Get("Settings.EquatorialGrid"), "IconEquatorialGrid", settings, "EquatorialGrid", "Grids"));
            AddToolbarItem(new ToolbarButton(Text.Get("Settings.HorizontalGrid"), "IconHorizontalGrid", settings, "HorizontalGrid", "Grids"));

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
