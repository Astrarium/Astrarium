using Astrarium.Plugins.Grids.Controls;
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
            DefineSetting("EquatorialGrid", false);
            DefineSetting("LabelEquatorialPoles", true);
            DefineSetting("HorizontalGrid", false);
            DefineSetting("LabelHorizontalPoles", true);
            DefineSetting("EclipticLine", true);
            DefineSetting("LabelEquinoxPoints", false);
            DefineSetting("LabelLunarNodes", false);
            DefineSetting("GalacticEquator", true);
            DefineSetting("MeridianLine", false);

            DefineSetting("ColorEcliptic", Color.FromArgb(0x80, 0x80, 0x00));
            DefineSetting("ColorMeridian", Color.FromArgb(0x08, 0xA4, 0x6F));
            DefineSetting("ColorGalacticEquator", Color.FromArgb(64, 0, 64));
            DefineSetting("ColorHorizontalGrid", Color.FromArgb(0x00, 0x40, 0x00));
            DefineSetting("ColorEquatorialGrid", Color.FromArgb(0, 64, 64));

            ToolbarItems.Add("Grids", new ToolbarToggleButton("IconHorizontalGrid", "$Settings.HorizontalGrid", new SimpleBinding(settings, "HorizontalGrid", "IsChecked")));
            ToolbarItems.Add("Grids", new ToolbarToggleButton("IconEquatorialGrid", "$Settings.EquatorialGrid", new SimpleBinding(settings, "EquatorialGrid", "IsChecked")));

            DefineSettingsSection<GridsSettingsSection, SettingsViewModel>();

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
