using Astrarium.Plugins.Satellites.Controls;
using Astrarium.Plugins.Satellites.ViewModels;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Astrarium.Plugins.Satellites
{
    public class SatellitesPlugin : AbstractPlugin
    {
        public SatellitesPlugin(ISettings settings)
        {
            DefineSetting("Satellites", true);
            DefineSetting("SatellitesLabels", true);
            DefineSetting("SatellitesShowOrbit", false);
            DefineSetting("SatellitesShowEclipsed", false);
            DefineSetting("SatellitesShowBelowHorizon", false);
            DefineSetting("SatellitesUseMagFilter", false);
            DefineSetting("SatellitesMagFilter", 4.0m);

            DefineSetting("ColorSatellitesOrbit", Color.FromArgb(255, 100, 0));
            DefineSetting("ColorSatellitesLabels", Color.FromArgb(255, 255, 0));
            DefineSetting("ColorEclipsedSatellitesLabels", Color.FromArgb(50, 50, 0));
            DefineSetting("SatellitesLabelsFont", new Font("Arial", 8));

            DefineSetting("SatellitesOrbitalElements", new List<TLESource>()
            {
                new TLESource() { FileName = "Brightest", Url = "https://celestrak.org/NORAD/elements/gp.php?GROUP=visual&FORMAT=tle", IsEnabled = true },
                new TLESource() { FileName = "SpaceStations", Url = "https://celestrak.org/NORAD/elements/gp.php?GROUP=stations&FORMAT=tle", IsEnabled = true }
            });

            DefineSettingsSection<SatellitesSettingsSection, SatellitesSettingsVM>();
            ExportResourceDictionaries("Images.xaml");

            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconSatellite", "$Settings.Satellites", new SimpleBinding(settings, "Satellites", "IsChecked")));
        }
    }
}
