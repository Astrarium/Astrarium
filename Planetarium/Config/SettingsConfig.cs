using Planetarium.Renderers;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Config
{
    public class SettingsConfig : AbstractSettingsConfig
    {
        /// <summary>
        /// Default settings going here
        /// </summary>
        public SettingsConfig()
        {
            AddSetting("EquatorialGrid", true, "Grids");
            AddSetting("LabelEquatorialPoles", true, "Grids", s => s.Get<bool>("EquatorialGrid"));
            AddSetting("HorizontalGrid", true, "Grids");
            AddSetting("LabelHorizontalPoles", true, "Grids", s => s.Get<bool>("HorizontalGrid"));
            AddSetting("EclipticLine", true, "Grids");

            AddSetting("ConstLabelsType", ConstellationsRenderer.LabelType.InternationalName, "Constellations");

            // settingsConfig.Add("LabelEquinoxPoints", false).EnabledWhenTrue("EclipticLine").WithSection("Grid");
            //settingsConfig.Add("LabelLunarNodes", false).EnabledWhenTrue("EclipticLine").WithSection("Grid");
            //settingsConfig.Add("GalacticEquator", true).WithSection("Grid");
            //settingsConfig.Add("MilkyWay", true).WithSection("Grid");
            //settingsConfig.Add("Ground", true).WithSection("Grid");
            //settingsConfig.Add("HorizonLine", true).WithSection("Grid");
            //settingsConfig.Add("LabelCardinalDirections", true).EnabledWhenTrue("HorizonLine").WithSection("Grid");

            //settingsConfig.Add("Sun", true).WithSection("Sun");
            //settingsConfig.Add("LabelSun", true).EnabledWhenTrue("Sun").WithSection("Sun");
            //settingsConfig.Add("SunLabelFont", new Font("Arial", 12)).EnabledWhen(s => s.Get<bool>("Sun") && s.Get<bool>("LabelSun")).WithSection("Sun");
            //settingsConfig.Add("TextureSun", true).EnabledWhenTrue("Sun").WithSection("Sun");
            //settingsConfig.Add("TextureSunPath", "https://soho.nascom.nasa.gov/data/REPROCESSING/Completed/{yyyy}/hmiigr/{yyyy}{MM}{dd}/{yyyy}{MM}{dd}_0000_hmiigr_512.jpg").EnabledWhen(s => s.Get<bool>("Sun") && s.Get<bool>("TextureSun")).WithSection("Sun");

            //settingsConfig.Add("ConstLabels", true).WithSection("Constellations");
            //settingsConfig.Add("ConstLabelsType", ConstellationsRenderer.LabelType.InternationalName).EnabledWhenTrue("ConstLabels").WithSection("Constellations");
            //settingsConfig.Add("ConstLines", true).WithSection("Constellations");
            //settingsConfig.Add("ConstBorders", true).WithSection("Constellations");

            //settingsConfig.Add("Stars", true).WithSection("Stars");
            //settingsConfig.Add("StarsLabels", true).EnabledWhenTrue("Stars").WithSection("Stars");
            //settingsConfig.Add("StarsProperNames", true).EnabledWhen(s => s.Get<bool>("Stars") && s.Get<bool>("StarsLabels")).WithSection("Stars");

            //settingsConfig.Add("EclipticColorNight", Color.FromArgb(0xC8, 0x80, 0x80, 0x00)).WithSection("Colors");
            //settingsConfig.Add("HorizontalGridColorNight", Color.FromArgb(0xC8, 0x00, 0x40, 0x00)).WithSection("Colors");
            //settingsConfig.Add("CardinalDirectionsColor", Color.FromArgb(0x00, 0x99, 0x99)).WithSection("Colors");

            //settingsConfig.Add("UseTextures", true).WithSection("Misc");

            //settingsConfig.Add("Planets", true).WithSection("Planets");
            //settingsConfig.Add("JupiterMoonsShadowOutline", true).WithSection("Planets");
            //settingsConfig.Add("ShowRotationAxis", false).WithSection("Planets");

            //settingsConfig.Add("GRSLongitude", 
            //    new GreatRedSpotSettings()
            //    {
            //        Epoch = 2458150.5000179596,
            //        MonthlyDrift = 2,
            //        Longitude = 283
            //    })
            //    .WithSection("Planets")
            //    .WithBuilder(typeof(GRSSettingBuilder));

            //settingsConfig.Add("Comets", true).WithSection("Comets");
            //settingsConfig.Add("CometsLabels", true).WithSection("Comets").EnabledWhenTrue("Comets");
            //settingsConfig.Add("Asteroids", true).WithSection("Asteroids");
            //settingsConfig.Add("AsteroidsLabels", true).WithSection("Asteroids").EnabledWhenTrue("Asteroids");

            //settingsConfig.Add("DeepSky", true).WithSection("Deep Sky");
            //settingsConfig.Add("DeepSkyLabels", true).WithSection("Deep Sky").EnabledWhenTrue("DeepSky");
            //settingsConfig.Add("DeepSkyOutlines", true).WithSection("Deep Sky").EnabledWhenTrue("DeepSky");

            //settingsConfig.Add("ObserverLocation", new CrdsGeographical(-44, 56.3333, +3, 80, "Europe/Moscow", "Nizhny Novgorod"));

        }
    }
}
