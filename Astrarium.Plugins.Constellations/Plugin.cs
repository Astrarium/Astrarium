using Astrarium.Plugins.Constellations.Controls;
using Astrarium.Plugins.Constellations.ViewModels;
using Astrarium.Types;
using System.Drawing;

namespace Astrarium.Plugins.Constellations
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            DefineSetting("ConstLines", true);
            DefineSetting("ConstBorders", true);
            DefineSetting("ConstLabels", true);
            DefineSetting("ConstLabelsType", ConstellationsRenderer.LabelType.InternationalName);
            DefineSetting("ConstLinesType", ConstellationsCalc.LineType.Traditional);
            DefineSetting("ConstFigures", false);
            DefineSetting("ConstFiguresType", ConstellationsRenderer.FigureType.Hevelius);
            DefineSetting("ConstFiguresGroup", ConstellationsRenderer.FigureGroup.All);
            
            DefineSetting("ConstFiguresHighlightHovered", true);
            DefineSetting("ConstFiguresDimOnZoom", true);

            // Colors
            DefineSetting("ColorConstLines", Color.Gray);
            DefineSetting("ColorConstBorders", Color.IndianRed);
            DefineSetting("ColorConstLabels", Color.IndianRed);

            // Fonts
            DefineSetting("ConstLabelsFont", new Font("Arial", 32));

            ToolbarItems.Add("Constellations", new ToolbarToggleButton("IconConstLines", "$Settings.ConstLines", new SimpleBinding(settings, "ConstLines", "IsChecked")));
            ToolbarItems.Add("Constellations", new ToolbarToggleButton("IconConstBorders", "$Settings.ConstBorders", new SimpleBinding(settings, "ConstBorders", "IsChecked")));
            ToolbarItems.Add("Constellations", new ToolbarToggleButton("IconConstLabels", "$Settings.ConstLabels", new SimpleBinding(settings, "ConstLabels", "IsChecked")));
            ToolbarItems.Add("Constellations", new ToolbarToggleButton("IconConstFigures", "$Settings.ConstFigures", new SimpleBinding(settings, "ConstFigures", "IsChecked")));

            DefineSettingsSection<ConstellationsSettingsSection, ConstellationsSettingsVM>();

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
