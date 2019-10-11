using Planetarium.Config;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.Horizon
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            #region Settings

            AddSetting(new SettingItem("Ground", true, "Grids"));
            AddSetting(new SettingItem("HorizonLine", true, "Grids"));
            AddSetting(new SettingItem("LabelCardinalDirections", true, "Grids", s => s.Get<bool>("HorizonLine")));

            AddSetting(new SettingItem("CardinalDirectionsColor", Color.FromArgb(0x00, 0x99, 0x99), "Colors"));

            AddSetting(new SettingItem("ColorGround", new SkyColor() { Night = Color.FromArgb(4, 10, 10), Day = Color.FromArgb(116, 185, 139), White = Color.White }));
            AddSetting(new SettingItem("ColorHorizon", new SkyColor() { Night = Color.FromArgb(0xC8, 0x00, 0x40, 0x00), Day = Color.FromArgb(116, 185, 139), White = Color.Black }));

            #endregion Settings

            AddToolbarItem(new ToolbarButton("Ground", "IconGround", settings, "Ground", "Grids"));

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
