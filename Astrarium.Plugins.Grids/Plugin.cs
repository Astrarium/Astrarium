using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Astrarium.Plugins.Grids
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            SettingItems.Add("Grids", new SettingItem("EquatorialGrid", false));
            SettingItems.Add("Grids", new SettingItem("LabelEquatorialPoles", true, s => s.Get<bool>("EquatorialGrid")));
            SettingItems.Add("Grids", new SettingItem("HorizontalGrid", false));
            SettingItems.Add("Grids", new SettingItem("LabelHorizontalPoles", true, s => s.Get<bool>("HorizontalGrid")));
            SettingItems.Add("Grids", new SettingItem("EclipticLine", true));
            SettingItems.Add("Grids", new SettingItem("LabelEquinoxPoints", false, s => s.Get<bool>("EclipticLine")));
            SettingItems.Add("Grids", new SettingItem("LabelLunarNodes", false, s => s.Get<bool>("EclipticLine")));
            SettingItems.Add("Grids", new SettingItem("GalacticEquator", true));
            SettingItems.Add("Colors", new SettingItem("ColorEcliptic", new SkyColor(0x80, 0x80, 0x00)));
            SettingItems.Add("Colors", new SettingItem("ColorGalacticEquator", new SkyColor(64, 0, 64)));
            SettingItems.Add("Colors", new SettingItem("ColorHorizontalGrid", new SkyColor(0x00, 0x40, 0x00)));
            SettingItems.Add("Colors", new SettingItem("ColorEquatorialGrid", new SkyColor(0, 64, 64)));

            ToolbarItems.Add("Grids", new ToolbarToggleButton("IconEquatorialGrid", "$Settings.EquatorialGrid", new SimpleBinding(settings, "EquatorialGrid", "IsChecked")));
            ToolbarItems.Add("Grids", new ToolbarToggleButton("IconHorizontalGrid", "$Settings.HorizontalGrid", new SimpleBinding(settings, "HorizontalGrid", "IsChecked")));

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
