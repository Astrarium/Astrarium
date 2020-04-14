using Astrarium.Algorithms;
using Astrarium.Config;
using Astrarium.Plugins.SolarSystem.Controls;
using Astrarium.Types;
using Astrarium.Types.Localization;
using System.ComponentModel;
using System.Drawing;

namespace Astrarium.Plugins.SolarSystem
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            #region Settings

            SettingItems.Add("Sun", new SettingItem("Sun", true, "Sun"));
            SettingItems.Add("Sun", new SettingItem("SunLabel", true, "Sun", s => s.Get("Sun")));
            SettingItems.Add("Sun", new SettingItem("SunTexture", true, "Sun", s => s.Get("Sun")));
            SettingItems.Add("Sun", new SettingItem("SunTexturePath", "https://soho.nascom.nasa.gov/data/REPROCESSING/Completed/{yyyy}/hmiigr/{yyyy}{MM}{dd}/{yyyy}{MM}{dd}_0000_hmiigr_512.jpg", "Sun", s => s.Get("Sun") && s.Get("SunTexture")));

            SettingItems.Add("Planets", new SettingItem("Planets", true, "Planets"));
            SettingItems.Add("Planets", new SettingItem("PlanetsLabels", true, "Planets", s => s.Get("Planets")));
            SettingItems.Add("Planets", new SettingItem("PlanetsTextures", true, "Planets", s => s.Get("Planets")));
            SettingItems.Add("Planets", new SettingItem("PlanetsSurfaceFeatures", true, "Planets", s => s.Get("Planets") && s.Get("PlanetsTextures")));
            SettingItems.Add("Planets", new SettingItem("PlanetsMartianPolarCaps", true, "Planets", s => s.Get("Planets") && s.Get("PlanetsTextures")));
            SettingItems.Add("Planets", new SettingItem("ShowRotationAxis", true, "Planets", s => s.Get("Planets")));
            SettingItems.Add("Planets", new SettingItem("PlanetMoons", true, "Planets", s => s.Get("Planets")));
            SettingItems.Add("Planets", new SettingItem("JupiterMoonsShadowOutline", true, "Planets", s => s.Get("Planets") && s.Get("PlanetMoons")));
            SettingItems.Add("Planets", new SettingItem("GenericMoons", true, "Planets", s => s.Get("Planets") && s.Get("PlanetMoons")));
            SettingItems.Add("Planets", new SettingItem("GenericMoonsAutoUpdate", false, "Planets", s => s.Get("Planets") && s.Get("PlanetMoons") && s.Get("GenericMoons")));
            SettingItems.Add("Planets", new SettingItem("GRSLongitude", new GreatRedSpotSettings()
            {
                Epoch = 2458150.5000179596,
                MonthlyDrift = 2,
                Longitude = 283
            }, "Planets", typeof(GRSSettingControl), s => s.Get("Planets")));

            SettingItems.Add("Moon", new SettingItem("Moon", true, "Moon"));
            SettingItems.Add("Moon", new SettingItem("MoonLabel", true, "Moon", s => s.Get("Moon")));
            SettingItems.Add("Moon", new SettingItem("MoonTexture", true, "Moon", s => s.Get("Moon")));
            SettingItems.Add("Moon", new SettingItem("MoonTextureQuality", TextureQuality.Normal, "Moon", s => s.Get("Moon") && s.Get("MoonTexture")));
            SettingItems.Add("Moon", new SettingItem("MoonSurfaceFeatures", true, "Moon", s => s.Get("Moon") && s.Get("MoonTexture")));
            SettingItems.Add("Moon", new SettingItem("EarthShadowOutline", false, "Moon", s => s.Get("Moon")));

            // Colors
            SettingItems.Add("Colors", new SettingItem("ColorSolarSystemLabel", Color.DimGray, "Colors"));

            #endregion Settings

            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconSun", "$Settings.Sun", new SimpleBinding(settings, "Sun", "IsChecked")));
            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconMoon", "$Settings.Moon", new SimpleBinding(settings, "Moon", "IsChecked")));
            ToolbarItems.Add("Objects", new ToolbarToggleButton("IconPlanet", "$Settings.Planets", new SimpleBinding(settings, "Planets", "IsChecked")));

            ExportResourceDictionaries("Images.xaml");
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
}
