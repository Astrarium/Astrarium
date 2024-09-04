using Astrarium.Algorithms;
using Astrarium.Plugins.Horizon.Controls;
using Astrarium.Plugins.Horizon.ViewModels;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Horizon
{
    public class HorizonPlugin : AbstractPlugin
    {
        private readonly ISettings settings = null;

        public HorizonPlugin(ISettings settings)
        {
            this.settings = settings;

            DefineSetting("Ground", true);
            DefineSetting("Landscape", "Derechin");
            DefineSetting("UseLandscape", true);
            DefineSetting("Landmarks", false);
            DefineSetting("HorizonLine", true);
            DefineSetting("GroundTextureNightDimming", 90m);
            DefineSetting("GroundTransparency", 0m);
            DefineSetting("LabelCardinalDirections", true);
            DefineSetting("MeasureAzimuthFromNorth", false);

            // Colors
            DefineSetting("ColorCardinalDirections", Color.SeaGreen);
            DefineSetting("ColorHorizon", Color.DarkGreen);

            // Fonts
            DefineSetting("CardinalDirectionsFont", new Font("Arial", 14, FontStyle.Bold));

            ToolbarItems.Add("Ground", new ToolbarToggleButton("IconGround", "$Settings.Ground", new SimpleBinding(settings, "Ground", "IsChecked")));

            DefineSettingsSection<HorizonSettingsSection, HorizonSettingsViewModel>();

            ExportResourceDictionaries("Images.xaml");

            settings.SettingValueChanged += (settingName, value) =>
            {
                 if (settingName == "MeasureAzimuthFromNorth")
                 {
                     SetAzimuthOrigin((bool)value);
                 }
            };
        }

        public override void Initialize()
        {
            SetAzimuthOrigin(settings.Get("MeasureAzimuthFromNorth"));
        }

        private void SetAzimuthOrigin(bool fromNorth)
        {
            CrdsHorizontal.MeasureAzimuthFromNorth = fromNorth;
        }
    }
}
