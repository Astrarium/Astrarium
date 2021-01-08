using Astrarium.Algorithms;
using Astrarium.Types;
using Astrarium.Types.Controls;
using System.ComponentModel;
using System.Drawing;

namespace Astrarium.Plugins.Eclipses
{
    public class Plugin : AbstractPlugin
    {
        public Plugin(ISettings settings)
        {
            #region UI integration

            MenuItem eclipsesMenu = new MenuItem("$Astrarium.Plugins.Eclipses.ToolsMenu");
            MenuItem solarEclipsesMenu = new MenuItem("$Astrarium.Plugins.SolarEclipses.ToolsMenu", new Command(ShowSolarEclipsesView));
            MenuItem lunarEclipsesMenu = new MenuItem("$Astrarium.Plugins.LunarEclipses.ToolsMenu", new Command(ShowLunarEclipsesView));
            eclipsesMenu.SubItems.Add(solarEclipsesMenu);
            eclipsesMenu.SubItems.Add(lunarEclipsesMenu);

            MenuItems.Add(MenuItemPosition.MainMenuTools, eclipsesMenu);

            #endregion UI integration

            SettingItems.Add(null, new SettingItem("EclipseMapTileServer", ""));
        }

        private void ShowSolarEclipsesView()
        {
            var vm = ViewManager.CreateViewModel<SolarEclipseVM>();
            ViewManager.ShowWindow(vm);
        }

        private void ShowLunarEclipsesView()
        {
            ViewManager.ShowMessageBox("Info", "Not implemented yet.");
        }
    }
}
