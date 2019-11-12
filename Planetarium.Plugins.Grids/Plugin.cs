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
using System.Windows.Data;

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

            AddSetting(new SettingItem("ColorEcliptic", Color.FromArgb(0xC8, 0x80, 0x80, 0x00), "Colors"));
            AddSetting(new SettingItem("ColorGalacticEquator", Color.FromArgb(200, 64, 0, 64), "Colors"));

            AddSetting(new SettingItem("ColorHorizontalGrid", Color.FromArgb(0xC8, 0x00, 0x40, 0x00), "Colors"));
            AddSetting(new SettingItem("ColorEquatorialGrid", Color.FromArgb(200, 0, 64, 64), "Colors"));

            AddToolbarItem(new ToolbarToggleButton("Settings.EquatorialGrid", "IconEquatorialGrid", new SimpleBinding(settings, "EquatorialGrid"), "Grids"));
            AddToolbarItem(new ToolbarToggleButton("Settings.HorizontalGrid", "IconHorizontalGrid", new SimpleBinding(settings, "HorizontalGrid"), "Grids"));

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
