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
    public class SettingsConfig : List<SettingItem>
    {
        /// <summary>
        /// Default settings going here
        /// </summary>
        public SettingsConfig()
        {
            Add(new SettingItem("EquatorialGrid", true, "Grids"));
            Add(new SettingItem("LabelEquatorialPoles", true, "Grids", s => s.Get<bool>("EquatorialGrid")));
            Add(new SettingItem("HorizontalGrid", true, "Grids"));
            Add(new SettingItem("LabelHorizontalPoles", true, "Grids", s => s.Get<bool>("HorizontalGrid")));
            Add(new SettingItem("EclipticLine", true, "Grids"));
            Add(new SettingItem("LabelEquinoxPoints", false, "Grids", s => s.Get<bool>("EclipticLine")));
            Add(new SettingItem("LabelLunarNodes", false, "Grids", s => s.Get<bool>("EclipticLine")));
            Add(new SettingItem("GalacticEquator", true, "Grids"));

            Add(new SettingItem("MilkyWay", true, "Grids"));
            Add(new SettingItem("Ground", true, "Grids"));
            Add(new SettingItem("HorizonLine", true, "Grids"));
            Add(new SettingItem("LabelCardinalDirections", true, "Grids", s => s.Get<bool>("HorizonLine")));

            Add(new SettingItem("Sun", true, "Sun"));
            Add(new SettingItem("LabelSun", true, "Sun", s => s.Get<bool>("Sun")));
            Add(new SettingItem("SunLabelFont", new Font("Arial", 12), "Sun", s => s.Get<bool>("Sun") && s.Get<bool>("LabelSun")));
            Add(new SettingItem("TextureSun", true, "Sun", s => s.Get<bool>("Sun")));
            Add(new SettingItem("TextureSunPath", "https://soho.nascom.nasa.gov/data/REPROCESSING/Completed/{yyyy}/hmiigr/{yyyy}{MM}{dd}/{yyyy}{MM}{dd}_0000_hmiigr_512.jpg", "Sun", s => s.Get<bool>("Sun") && s.Get<bool>("TextureSun")));

            Add(new SettingItem("ConstLabels", true, "Constellations"));
            Add(new SettingItem("ConstLabelsType", ConstellationsRenderer.LabelType.InternationalName, "Constellations", s => s.Get<bool>("ConstLabels")));
            Add(new SettingItem("ConstLines", true, "Constellations"));
            Add(new SettingItem("ConstBorders", true, "Constellations"));

            Add(new SettingItem("Stars", true, "Stars"));
            Add(new SettingItem("StarsLabels", true, "Stars", s => s.Get<bool>("Stars")));
            Add(new SettingItem("StarsProperNames", true, "Stars", s => s.Get<bool>("Stars") && s.Get<bool>("StarsLabels")));
            Add(new SettingItem("EclipticColorNight", Color.FromArgb(0xC8, 0x80, 0x80, 0x00), "Colors"));
            Add(new SettingItem("HorizontalGridColorNight", Color.FromArgb(0xC8, 0x00, 0x40, 0x00), "Colors"));
            Add(new SettingItem("CardinalDirectionsColor", Color.FromArgb(0x00, 0x99, 0x99), "Colors"));

            Add(new SettingItem("Planets", true, "Planets"));
            Add(new SettingItem("UseTextures", true, "Planets", s => s.Get<bool>("Planets")));
            Add(new SettingItem("JupiterMoonsShadowOutline", true, "Planets", s => s.Get<bool>("Planets")));
            Add(new SettingItem("ShowRotationAxis", true, "Planets", s => s.Get<bool>("Planets")));

            Add(new SettingItem("GRSLongitude", new GreatRedSpotSettings()
            {
                Epoch = 2458150.5000179596,
                MonthlyDrift = 2,
                Longitude = 283
            }, "Planets", typeof(GRSSettingControl), s => s.Get<bool>("Planets")));


            // setting without editor in settings window
            Add(new SettingItem("ObserverLocation", new CrdsGeographical(-44, 56.3333, +3, 80, "Europe/Moscow", "Nizhny Novgorod")));

            Add(new SettingItem("RenderingOrder", new List<RendererDescription>(), "Rendering", typeof(RenderersListSettingControl)));
        }
    }
}
