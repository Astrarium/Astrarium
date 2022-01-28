using Astrarium.Plugins.MinorBodies.Controls;
using Astrarium.Plugins.MinorBodies.ViewModels;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.MinorBodies
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            DefineSetting("Comets", true);
            DefineSetting("CometsLabels", true);
            DefineSetting("CometsLabelsMag", false);
            DefineSetting("CometsDrawAll", false);
            DefineSetting("CometsDrawAllMagLimit", 10m);

            DefineSetting("CometsAutoUpdateOrbitalElements", false);
            DefineSetting("CometsAutoUpdateOrbitalElementsPeriod", 30m);
            DefineSetting("CometsDownloadOrbitalElementsCount", 1000m);
            DefineSetting("CometsDownloadOrbitalElementsUrl", "https://www.minorplanetcenter.net/iau/MPCORB/CometEls.txt");
            DefineSetting("CometsDownloadOrbitalElementsTimestamp", DateTime.MinValue, isPermanent: true);

            DefineSetting("Asteroids", true);
            DefineSetting("AsteroidsLabels", true);
            DefineSetting("AsteroidsLabelsMag", false);
            DefineSetting("AsteroidsDrawAll", false);
            DefineSetting("AsteroidsDrawAllMagLimit", 10m);

            DefineSetting("AsteroidsAutoUpdateOrbitalElements", false);
            DefineSetting("AsteroidsAutoUpdateOrbitalElementsPeriod", 30m);
            DefineSetting("AsteroidsDownloadOrbitalElementsCount", 1000m);
            DefineSetting("AsteroidsDownloadOrbitalElementsUrl", "https://www.minorplanetcenter.net/iau/MPCORB/MPCORB.DAT");
            DefineSetting("AsteroidsDownloadOrbitalElementsTimestamp", DateTime.MinValue, isPermanent: true);

            DefineSetting("ColorAsteroidsLabels", new SkyColor(10, 44, 37));
            DefineSetting("ColorCometsLabels", new SkyColor(78, 84, 99));

            DefineSetting("AsteroidsLabelsFont", new Font("Arial", 8));
            DefineSetting("CometsLabelsFont", new Font("Arial", 8));

            DefineSettingsSection<CometsSettingsSection, CometsSettingsVM>();
            DefineSettingsSection<AsteroidsSettingsSection, AsteroidsSettingsVM>();

            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconAsteroid", "$Settings.Asteroids", new SimpleBinding(settings, "Asteroids", "IsChecked")));
            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconComet", "$Settings.Comets", new SimpleBinding(settings, "Comets", "IsChecked")));

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
