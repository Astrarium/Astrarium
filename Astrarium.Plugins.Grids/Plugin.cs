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

            DefineSetting("ColorEcliptic", Color.Goldenrod);
            DefineSetting("ColorMeridian", Color.SpringGreen);
            DefineSetting("ColorGalacticEquator", Color.Fuchsia);
            DefineSetting("ColorHorizontalGrid", Color.Green);
            DefineSetting("ColorEquatorialGrid", Color.DarkCyan);

            ToolbarItems.Add("Grids", new ToolbarToggleButton("IconHorizontalGrid", "$Settings.HorizontalGrid", new SimpleBinding(settings, "HorizontalGrid", "IsChecked")));
            ToolbarItems.Add("Grids", new ToolbarToggleButton("IconEquatorialGrid", "$Settings.EquatorialGrid", new SimpleBinding(settings, "EquatorialGrid", "IsChecked")));

            DefineSettingsSection<GridsSettingsSection, SettingsViewModel>();

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
