using Astrarium.Algorithms;
using Astrarium.Plugins.SolarSystem.Controls;
using Astrarium.Plugins.SolarSystem.Objects;
using Astrarium.Plugins.SolarSystem.ViewModels;
using Astrarium.Types;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Controls;

namespace Astrarium.Plugins.SolarSystem
{
    public class SolarSystemPlugin : AbstractPlugin
    {
        public SolarSystemPlugin(ISettings settings)
        {
            #region Settings

            DefineSetting("Sun", true);
            DefineSetting("SunLabel", true);
            DefineSetting("SunTexture", true);
            DefineSetting("SunFeatures", true);
            DefineSetting("SunEquator", false);

            DefineSetting("Planets", true);
            DefineSetting("PlanetsDrawAll", false);
            DefineSetting("PlanetsLabels", true);
            DefineSetting("PlanetsLabelsMag", false);

            DefineSetting("PlanetsSurfaceFeatures", true);
            DefineSetting("PlanetsMartianPolarCaps", true);

            DefineSetting("PlanetMoons", true);
            DefineSetting("JupiterMoonsShadowOutline", true);
            DefineSetting("GenericMoons", true);
            DefineSetting("GenericMoonsAutoUpdate", false);
            DefineSetting("GenericMoonsOrbitalElementsValidity", 30m);
            DefineSetting("GenericMoonsOrbitalElementsLastUpdated", DateTime.MinValue);

            DefineSetting("GRSLongitude", new GreatRedSpotSettings()
            {
                Epoch = 2460492.5,
                MonthlyDrift = 1.54,
                Longitude = 57
            }, isPermanent: true);

            DefineSetting("Moon", true);
            DefineSetting("MoonLabel", true);
            DefineSetting("MoonMaxLibrationPoint", false);
            DefineSetting("MoonPrimeMeridian", false);
            DefineSetting("MoonEquator", false);
            DefineSetting("MoonTextureQuality", TextureQuality.High);
            DefineSetting("MoonSurfaceFeatures", true);
            DefineSetting("EarthShadowOutline", false);

            // Colors
            DefineSetting("ColorSolarSystemLabel", Color.LightGray);

            // Fonts
            DefineSetting("SolarSystemLabelsFont", new Font("Arial", 9));

            #endregion Settings

            #region UI integration

            DefineSettingsSection<SunSettingsSection, SettingsViewModel>();
            DefineSettingsSection<MoonSettingsSection, MoonSettingsVM>();
            DefineSettingsSection<PlanetsSettingsSection, PlanetsSettingsVM>();

            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconSun", "$Settings.Sun", new SimpleBinding(settings, "Sun", "IsChecked")));
            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconMoon", "$Settings.Moon", new SimpleBinding(settings, "Moon", "IsChecked")));
            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconPlanet", "$Settings.Planets", new SimpleBinding(settings, "Planets", "IsChecked")));

            ExportResourceDictionaries("Images.xaml");

            ExtendObjectInfo<SolarActivityControl, SolarActivityViewModel>("$SolarActivity.ObjectInfoExtension.Title", GetSolarActivityViewModel);

            #endregion UI integration

            #region Extending formatters

            Formatters.Default["Appearance.CM"] = new UnsignedDoubleFormatter(2, "\u00B0");
            Formatters.Default["Appearance.P"] = new UnsignedDoubleFormatter(2, "\u00B0");
            Formatters.Default["Appearance.D"] = new UnsignedDoubleFormatter(2, "\u00B0");

            #endregion Extending formatters
        }

        private SolarActivityViewModel GetSolarActivityViewModel(SkyContext ctx, CelestialObject obj)
        {
            if (obj is Sun)
            {
                var vm = ViewManager.CreateViewModel<SolarActivityViewModel>();
                vm.SetDate(ctx.JulianDay, ctx.GeoLocation.UtcOffset);
                return vm;
            }
            else
            {
                return null;
            }
        }
    }

    public enum TextureQuality
    {
        [Description("Settings.MoonTextureQuality.Low")]
        Low = 2,

        [Description("Settings.MoonTextureQuality.Normal")]
        Normal = 4,

        [Description("Settings.MoonTextureQuality.High")]
        High = 8
    }
}
