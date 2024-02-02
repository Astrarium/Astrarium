﻿using Astrarium.Plugins.Satellites.Controls;
using Astrarium.Plugins.Satellites.ViewModels;
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
            DefineSetting("SatellitesShowOrbit", false);
            DefineSetting("SatellitesShowEclipsed", false);
            DefineSetting("SatellitesShowBelowHorizon", false);

            DefineSetting("ColorSatellitesOrbit", Color.FromArgb(255, 100, 0));
            DefineSetting("ColorSatellitesLabels", Color.FromArgb(255, 255, 0));
            DefineSetting("ColorEclipsedSatellitesLabels", Color.FromArgb(50, 50, 0));
            DefineSetting("SatellitesLabelsFont", new Font("Arial", 8));
            DefineSettingsSection<SatellitesSettingsSection, SatellitesSettingsVM>();
            ExportResourceDictionaries("Images.xaml");
        }
    }
}