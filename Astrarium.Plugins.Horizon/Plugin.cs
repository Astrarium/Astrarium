using Astrarium.Config;
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
        public Plugin(ISettings settings)
        {
            #region Settings

            SettingItems.Add("Grids", new SettingItem("Ground", true, "Grids"));
            SettingItems.Add("Grids", new SettingItem("HorizonLine", true, "Grids"));
            SettingItems.Add("Grids", new SettingItem("LabelCardinalDirections", true, "Grids", s => s.Get<bool>("HorizonLine")));

            SettingItems.Add("Colors", new SettingItem("ColorCardinalDirections", Color.FromArgb(0x00, 0x99, 0x99), "Colors"));
            SettingItems.Add("Colors", new SettingItem("ColorHorizon", Color.FromArgb(0xC8, 0x00, 0x40, 0x00), "Colors"));

            #endregion Settings

            ToolbarItems.Add("Grids", new ToolbarToggleButton("IconGround", "$Settings.Ground", new SimpleBinding(settings, "Ground", "IsChecked")));

            ExportResourceDictionaries("Images.xaml");
        }
    }
}
