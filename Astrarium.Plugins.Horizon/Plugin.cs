using Astrarium.Algorithms;
using Astrarium.Plugins.Horizon.Controls;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Horizon
{
    public class Plugin : AbstractPlugin
    {
        private readonly ISettings settings = null;

        public Plugin(ISettings settings)
        {
            this.settings = settings;

            DefineSetting("Ground", true);
            DefineSetting("HorizonLine", true);
            DefineSetting("GroundTextureNightDimming", 90m);
            DefineSetting("LabelCardinalDirections", true);
            DefineSetting("MeasureAzimuthFromNorth", false);

            // Colors
            DefineSetting("ColorCardinalDirections", new SkyColor(0x00, 0x99, 0x99));
            DefineSetting("ColorHorizon", new SkyColor(0x00, 0x40, 0x00));

            // Fonts
            DefineSetting("CardinalDirectionsFont", new Font("Arial", 12));

            ToolbarItems.Add("Ground", new ToolbarToggleButton("IconGround", "$Settings.Ground", new SimpleBinding(settings, "Ground", "IsChecked")));

            DefineSettingsSection<HorizonSettingsSection, SettingsViewModel>();

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
