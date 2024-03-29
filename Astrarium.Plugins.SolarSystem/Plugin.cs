﻿using Astrarium.Algorithms;
using Astrarium.Plugins.SolarSystem.Controls;
using Astrarium.Plugins.SolarSystem.ViewModels;
using Astrarium.Types;
using Astrarium.Types.Controls;
using System.ComponentModel;
using System.Drawing;

namespace Astrarium.Plugins.SolarSystem
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            #region Settings

            DefineSetting("Sun", true);
            DefineSetting("SunLabel", true);
            DefineSetting("SunTexture", true);

            DefineSetting("Planets", true);
            DefineSetting("PlanetsDrawAll", false);
            DefineSetting("PlanetsLabels", true);
            DefineSetting("PlanetsLabelsMag", false);

            DefineSetting("PlanetsTextures", true);
            DefineSetting("PlanetsSurfaceFeatures", true);
            DefineSetting("PlanetsMartianPolarCaps", true);
            DefineSetting("ShowRotationAxis", true);

            DefineSetting("PlanetMoons", true);
            DefineSetting("JupiterMoonsShadowOutline", true);
            DefineSetting("GenericMoons", true);
            DefineSetting("GenericMoonsAutoUpdate", false);
            DefineSetting("GenericMoonsOrbitalElementsValidity", 30m);

            DefineSetting("GRSLongitude", new GreatRedSpotSettings()
            {
                Epoch = 2458150.5000179596,
                MonthlyDrift = 2,
                Longitude = 283
            }, isPermanent: true);

            DefineSetting("Moon", true);
            DefineSetting("MoonLabel", true);
            DefineSetting("MoonPhase", true);
            DefineSetting("MoonTexture", true);
            DefineSetting("MoonTextureQuality", TextureQuality.Normal);
            DefineSetting("MoonSurfaceFeatures", true);
            DefineSetting("EarthShadowOutline", false);

            // Colors
            DefineSetting("ColorSolarSystemLabel", new SkyColor(Color.DimGray));

            // Fonts
            DefineSetting("SolarSystemLabelsFont", new Font("Arial", 8));

            #endregion Settings

            #region UI integration

            DefineSettingsSection<SunSettingsSection, SettingsViewModel>();
            DefineSettingsSection<MoonSettingsSection, MoonSettingsVM>();
            DefineSettingsSection<PlanetsSettingsSection, PlanetsSettingsVM>();

            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconSun", "$Settings.Sun", new SimpleBinding(settings, "Sun", "IsChecked")));
            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconMoon", "$Settings.Moon", new SimpleBinding(settings, "Moon", "IsChecked")));
            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconPlanet", "$Settings.Planets", new SimpleBinding(settings, "Planets", "IsChecked")));

            ExportResourceDictionaries("Images.xaml");

            #endregion UI integration

            #region Extending formatters

            Formatters.Default["Appearance.CM"] = new Formatters.UnsignedDoubleFormatter(2, "\u00B0");
            Formatters.Default["Appearance.P"] = new Formatters.UnsignedDoubleFormatter(2, "\u00B0");
            Formatters.Default["Appearance.D"] = new Formatters.UnsignedDoubleFormatter(2, "\u00B0");

            #endregion Extending formatters
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
