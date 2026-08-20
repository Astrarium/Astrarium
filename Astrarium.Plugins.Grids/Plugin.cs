using Astrarium.Plugins.Grids.Controls;
using Astrarium.Types;
using System.Drawing;

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

            ExportResourceDictionaries("Images.xaml");

            ToolbarItems.Add("Grids", new ToolbarToggleButton("IconHorizontalGrid", "$Settings.HorizontalGrid", new SimpleBinding(settings, "HorizontalGrid", "IsChecked")));
            ToolbarItems.Add("Grids", new ToolbarToggleButton("IconEquatorialGrid", "$Settings.EquatorialGrid", new SimpleBinding(settings, "EquatorialGrid", "IsChecked")));

            DefineSettingsSection<GridsSettingsSection, SettingsViewModel>();
        }
    }
}
