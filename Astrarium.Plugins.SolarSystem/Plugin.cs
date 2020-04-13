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

            AddSetting(new SettingItem("Sun", true, "Sun"));
            AddSetting(new SettingItem("SunLabel", true, "Sun", s => s.Get("Sun")));
            AddSetting(new SettingItem("SunTexture", true, "Sun", s => s.Get("Sun")));
            AddSetting(new SettingItem("SunTexturePath", "https://soho.nascom.nasa.gov/data/REPROCESSING/Completed/{yyyy}/hmiigr/{yyyy}{MM}{dd}/{yyyy}{MM}{dd}_0000_hmiigr_512.jpg", "Sun", s => s.Get("Sun") && s.Get("SunTexture")));

            AddSetting(new SettingItem("Planets", true, "Planets"));
            AddSetting(new SettingItem("PlanetsLabels", true, "Planets", s => s.Get("Planets")));
            AddSetting(new SettingItem("PlanetsTextures", true, "Planets", s => s.Get("Planets")));
            AddSetting(new SettingItem("PlanetsSurfaceFeatures", true, "Planets", s => s.Get("Planets") && s.Get("PlanetsTextures")));
            AddSetting(new SettingItem("PlanetsMartianPolarCaps", true, "Planets", s => s.Get("Planets") && s.Get("PlanetsTextures")));
            AddSetting(new SettingItem("ShowRotationAxis", true, "Planets", s => s.Get("Planets")));

            AddSetting(new SettingItem("PlanetMoons", true, "Planets", s => s.Get("Planets")));
            AddSetting(new SettingItem("JupiterMoonsShadowOutline", true, "Planets", s => s.Get("Planets") && s.Get("PlanetMoons")));            
            AddSetting(new SettingItem("GenericMoons", true, "Planets", s => s.Get("Planets") && s.Get("PlanetMoons")));
            AddSetting(new SettingItem("GenericMoonsAutoUpdate", false, "Planets", s => s.Get("Planets") && s.Get("PlanetMoons") && s.Get("GenericMoons")));

            AddSetting(new SettingItem("Moon", true, "Moon"));
            AddSetting(new SettingItem("MoonLabel", true, "Moon", s => s.Get("Moon")));
            AddSetting(new SettingItem("MoonTexture", true, "Moon", s => s.Get("Moon")));
            AddSetting(new SettingItem("MoonTextureQuality", TextureQuality.Normal, "Moon", s => s.Get("Moon") && s.Get("MoonTexture")));
            AddSetting(new SettingItem("MoonSurfaceFeatures", true, "Moon", s => s.Get("Moon") && s.Get("MoonTexture")));
            AddSetting(new SettingItem("EarthShadowOutline", false, "Moon", s => s.Get("Moon")));

            AddSetting(new SettingItem("GRSLongitude", new GreatRedSpotSettings()
            {
                Epoch = 2458150.5000179596,
                MonthlyDrift = 2,
                Longitude = 283
            }, "Planets", typeof(GRSSettingControl), s => s.Get("Planets")));

            // Colors

            AddSetting(new SettingItem("ColorSolarSystemLabel", Color.DimGray, "Colors"));

            #endregion Settings

            AddToolbarItem(new ToolbarToggleButton("IconSun", "$Settings.Sun", new SimpleBinding(settings, "Sun", "IsChecked"), "Objects"));
            AddToolbarItem(new ToolbarToggleButton("IconMoon", "$Settings.Moon", new SimpleBinding(settings, "Moon", "IsChecked"), "Objects"));
            AddToolbarItem(new ToolbarToggleButton("IconPlanet", "$Settings.Planets", new SimpleBinding(settings, "Planets", "IsChecked"), "Objects"));

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
