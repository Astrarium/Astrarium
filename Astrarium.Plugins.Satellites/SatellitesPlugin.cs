using Astrarium.Types;
using System;
using System.Drawing;

namespace Astrarium.Plugins.Satellites
{
    public class SatellitesPlugin : AbstractPlugin
    {
        public SatellitesPlugin()
        {
            DefineSetting("Satellites", true);
            DefineSetting("SatellitesLabels", true);
            DefineSetting("ColorSatellitesLabels", Color.Yellow);
            DefineSetting("SatellitesLabelsFont", new Font("Arial", 8));

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
