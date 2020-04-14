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

            SettingItems.Add("Sun", new SettingItem("Sun", true));
            SettingItems.Add("Sun", new SettingItem("SunLabel", true, s => s.Get("Sun")));
            SettingItems.Add("Sun", new SettingItem("SunTexture", true, s => s.Get("Sun")));
            SettingItems.Add("Sun", new SettingItem("SunTexturePath", "https://soho.nascom.nasa.gov/data/REPROCESSING/Completed/{yyyy}/hmiigr/{yyyy}{MM}{dd}/{yyyy}{MM}{dd}_0000_hmiigr_512.jpg", s => s.Get("Sun") && s.Get("SunTexture")));

            SettingItems.Add("Planets", new SettingItem("Planets", true));
            SettingItems.Add("Planets", new SettingItem("PlanetsLabels", true, s => s.Get("Planets")));
            SettingItems.Add("Planets", new SettingItem("PlanetsTextures", true, s => s.Get("Planets")));
            SettingItems.Add("Planets", new SettingItem("PlanetsSurfaceFeatures", true, s => s.Get("Planets") && s.Get("PlanetsTextures")));
            SettingItems.Add("Planets", new SettingItem("PlanetsMartianPolarCaps", true, s => s.Get("Planets") && s.Get("PlanetsTextures")));
            SettingItems.Add("Planets", new SettingItem("ShowRotationAxis", true, s => s.Get("Planets")));
            SettingItems.Add("Planets", new SettingItem("PlanetMoons", true, s => s.Get("Planets")));
            SettingItems.Add("Planets", new SettingItem("JupiterMoonsShadowOutline", true, s => s.Get("Planets") && s.Get("PlanetMoons")));
            SettingItems.Add("Planets", new SettingItem("GenericMoons", true, s => s.Get("Planets") && s.Get("PlanetMoons")));
            SettingItems.Add("Planets", new SettingItem("GenericMoonsAutoUpdate", false, s => s.Get("Planets") && s.Get("PlanetMoons") && s.Get("GenericMoons")));
            SettingItems.Add("Planets", new SettingItem("GRSLongitude", new GreatRedSpotSettings()
            {
                Epoch = 2458150.5000179596,
                MonthlyDrift = 2,
                Longitude = 283
            }, typeof(GRSSettingControl), s => s.Get("Planets")));

            SettingItems.Add("Moon", new SettingItem("Moon", true));
            SettingItems.Add("Moon", new SettingItem("MoonLabel", true, s => s.Get("Moon")));
            SettingItems.Add("Moon", new SettingItem("MoonTexture", true, s => s.Get("Moon")));
            SettingItems.Add("Moon", new SettingItem("MoonTextureQuality", TextureQuality.Normal, s => s.Get("Moon") && s.Get("MoonTexture")));
            SettingItems.Add("Moon", new SettingItem("MoonSurfaceFeatures", true, s => s.Get("Moon") && s.Get("MoonTexture")));
            SettingItems.Add("Moon", new SettingItem("EarthShadowOutline", false, s => s.Get("Moon")));

            // Colors
            SettingItems.Add("Colors", new SettingItem("ColorSolarSystemLabel", Color.DimGray));

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
