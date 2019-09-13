using ADK;
using Planetarium.Renderers;
using Planetarium.Types;
using Planetarium.Types.Config.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
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
            AddSetting("LabelEquinoxPoints", false, "Grids", s => s.Get<bool>("EclipticLine"));
            AddSetting("LabelLunarNodes", false, "Grids", s => s.Get<bool>("EclipticLine"));
            AddSetting("GalacticEquator", true, "Grids");

            AddSetting("MilkyWay", true, "Grids");
            AddSetting("Ground", true, "Grids");
            AddSetting("HorizonLine", true, "Grids");
            AddSetting("LabelCardinalDirections", true, "Grids", s => s.Get<bool>("HorizonLine"));

            AddSetting("Sun", true, "Sun");
            AddSetting("LabelSun", true, "Sun", s => s.Get<bool>("Sun"));
            AddSetting("SunLabelFont", new Font("Arial", 12), "Sun", s => s.Get<bool>("Sun") && s.Get<bool>("LabelSun"));
            AddSetting("TextureSun", true, "Sun", s => s.Get<bool>("Sun"));
            AddSetting("TextureSunPath", "https://soho.nascom.nasa.gov/data/REPROCESSING/Completed/{yyyy}/hmiigr/{yyyy}{MM}{dd}/{yyyy}{MM}{dd}_0000_hmiigr_512.jpg", "Sun", s => s.Get<bool>("Sun") && s.Get<bool>("TextureSun"));

            AddSetting("ConstLabels", true, "Constellations");
            AddSetting("ConstLabelsType", ConstellationsRenderer.LabelType.InternationalName, "Constellations", s => s.Get<bool>("ConstLabels"));
            AddSetting("ConstLines", true, "Constellations");
            AddSetting("ConstBorders", true, "Constellations");

            AddSetting("Stars", true, "Stars");
            AddSetting("StarsLabels", true, "Stars", s => s.Get<bool>("Stars"));
            AddSetting("StarsProperNames", true, "Stars", s => s.Get<bool>("Stars") && s.Get<bool>("StarsLabels"));
            AddSetting("EclipticColorNight", Color.FromArgb(0xC8, 0x80, 0x80, 0x00), "Colors");
            AddSetting("HorizontalGridColorNight", Color.FromArgb(0xC8, 0x00, 0x40, 0x00), "Colors");
            AddSetting("CardinalDirectionsColor", Color.FromArgb(0x00, 0x99, 0x99), "Colors");

            AddSetting("Planets", true, "Planets");
            AddSetting("UseTextures", true, "Planets", s => s.Get<bool>("Planets"));
            AddSetting("JupiterMoonsShadowOutline", true, "Planets", s => s.Get<bool>("Planets"));
            AddSetting("ShowRotationAxis", true, "Planets", s => s.Get<bool>("Planets"));

            AddSetting("GRSLongitude", new GreatRedSpotSettings()
            {
                Epoch = 2458150.5000179596,
                MonthlyDrift = 2,
                Longitude = 283
            }, "Planets", typeof(GRSSettingControl), s => s.Get<bool>("Planets"));

            AddSetting("Comets", true, "Comets");
            AddSetting("CometsLabels", true, "Comets", s => s.Get<bool>("Comets"));

            AddSetting("Asteroids", true, "Asteroids");
            AddSetting("AsteroidsLabels", true, "Asteroids", s => s.Get<bool>("Asteroids"));

            AddSetting("DeepSky", true, "Deep Sky Objects");
            AddSetting("DeepSkyLabels", true, "Deep Sky Objects", s => s.Get<bool>("DeepSky"));
            AddSetting("DeepSkyOutlines", true, "Deep Sky Objects", s => s.Get<bool>("DeepSky"));

            // setting without editor in settings window
            AddSetting("ObserverLocation", new CrdsGeographical(-44, 56.3333, +3, 80, "Europe/Moscow", "Nizhny Novgorod"));
        }
    }
}
