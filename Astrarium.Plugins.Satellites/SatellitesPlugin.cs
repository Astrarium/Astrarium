using Astrarium.Plugins.Satellites.Controls;
using Astrarium.Plugins.Satellites.ViewModels;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Astrarium.Plugins.Satellites
{
    public class SatellitesPlugin : AbstractPlugin
    {
        private readonly ISatellitesCalculator calc;
        private readonly IOrbitalElementsUpdater updater;
        private readonly ISettings settings;

        private readonly string TLE_DIR;

        public SatellitesPlugin(ISatellitesCalculator calc, IOrbitalElementsUpdater updater, ISettings settings)
        {
            this.calc = calc;
            this.updater = updater;
            this.settings = settings;

            TLE_DIR = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Astrarium", "Satellites");

            DefineSetting("Satellites", true);
            DefineSetting("SatellitesLabels", true);
            DefineSetting("SatellitesShowOrbit", true);
            DefineSetting("SatellitesShowEclipsed", true);
            DefineSetting("SatellitesShowBelowHorizon", true);
            DefineSetting("SatellitesUseMagFilter", true);
            DefineSetting("SatellitesMagFilter", 4.0m);

            DefineSetting("ColorSatellitesOrbit", Color.FromArgb(255, 100, 0));
            DefineSetting("ColorSatellitesLabels", Color.FromArgb(255, 255, 0));
            DefineSetting("ColorEclipsedSatellitesLabels", Color.FromArgb(50, 50, 0));
            DefineSetting("SatellitesLabelsFont", new Font("Arial", 8));

            DefineSetting("SatellitesOrbitalElements", new List<TLESource>()
            {
                new TLESource() { FileName = "Brightest", Url = "https://celestrak.org/NORAD/elements/gp.php?GROUP=visual&FORMAT=tle", IsEnabled = true },
                new TLESource() { FileName = "SpaceStations", Url = "https://celestrak.org/NORAD/elements/gp.php?GROUP=stations&FORMAT=tle", IsEnabled = true }
            }, isPermanent: true);

            DefineSetting("SatellitesAutoUpdateOrbitalElements", true);
            DefineSetting("SatellitesAutoUpdateOrbitalElementsPeriod", 1m);

            DefineSettingsSection<SatellitesSettingsSection, SatellitesSettingsVM>();
            ExportResourceDictionaries("Images.xaml");

            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconSatellite", "$Settings.Satellites", new SimpleBinding(settings, "Satellites", "IsChecked")));

            updater.OrbitalElementsUpdated += Updater_OrbitalElementsUpdated;
        }

        private void Updater_OrbitalElementsUpdated(TLESource tleSource)
        {
            List<TLESource> tleSources = settings.Get<List<TLESource>>("SatellitesOrbitalElements");
            var existing = tleSources.FirstOrDefault(x => x.FileName == tleSource.FileName);
            if (existing != null)
            {
                existing.LastUpdated = DateTime.Now;
                settings.SetAndSave("SatellitesOrbitalElements", tleSources);
            }
            tleSource.LastUpdated = DateTime.Now;
            calc.LoadSatellites(TLE_DIR, tleSource);
        }

        public override async void Initialize()
        {
            // TLE sources from settings
            List<TLESource> tleSources = settings.Get<List<TLESource>>("SatellitesOrbitalElements");
            
            // user directory for satellites data exists and contains TLE files
            if (Directory.Exists(TLE_DIR) && Directory.EnumerateFiles(TLE_DIR, "*.tle").Any())
            {
                foreach (var tleSource in tleSources)
                {
                    if (File.Exists(Path.Combine(TLE_DIR, $"{tleSource.FileName}.tle")))
                    {
                        calc.LoadSatellites(TLE_DIR, tleSource);
                    }
                }
            }

            // auto update TLEs
            if (settings.Get<bool>("SatellitesAutoUpdateOrbitalElements"))
            {
                int days = (int)settings.Get<decimal>("SatellitesAutoUpdateOrbitalElementsPeriod");
                foreach (var tleSource in tleSources)
                {
                    if (tleSource.IsEnabled &&
                       (tleSource.LastUpdated == null || DateTime.Now.Subtract(tleSource.LastUpdated.Value).TotalDays >= days))
                    {
                        Log.Info($"Obital elements of satellites ({tleSource.FileName}) needs to be updated, updating...");
                        await updater.UpdateOrbitalElements(tleSource, silent: true);
                    }
                }
            }
        }
    }
}
