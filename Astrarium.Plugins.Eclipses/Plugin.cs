using Astrarium.Algorithms;
using Astrarium.Plugins.Eclipses.ViewModels;
using Astrarium.Types;
using Astrarium.Types.Controls;
using System.ComponentModel;
using System.Drawing;

namespace Astrarium.Plugins.Eclipses
{
    public class Plugin : AbstractPlugin
    {
        public Plugin()
        {
            #region UI integration

            MenuItem eclipsesMenu = new MenuItem("$Astrarium.Plugins.Eclipses.ToolsMenu");
            MenuItem solarEclipsesMenu = new MenuItem("$Astrarium.Plugins.SolarEclipses.ToolsMenu", new Command(ShowSolarEclipsesView));
            MenuItem lunarEclipsesMenu = new MenuItem("$Astrarium.Plugins.LunarEclipses.ToolsMenu", new Command(ShowLunarEclipsesView));

            eclipsesMenu.SubItems.Add(solarEclipsesMenu);
            eclipsesMenu.SubItems.Add(lunarEclipsesMenu);

            MenuItems.Add(MenuItemPosition.MainMenuTools, eclipsesMenu);

            #endregion UI integration

            DefineSetting(Settings.EclipseMapTileServer, "", isPermanent: true);
            DefineSetting(Settings.EclipseMapOverlayTileServer, "", isPermanent: true);
            DefineSetting(Settings.EclipseMapOverlayOpacity, 0.5f, isPermanent: true);
        }

        private void ShowSolarEclipsesView()
        {
            var vm = ViewManager.CreateViewModel<SolarEclipseVM>();
            vm.Closing += (e) => { vm.Dispose(); };
            ViewManager.ShowWindow(vm);
        }

        private void ShowLunarEclipsesView()
        {
            var vm = ViewManager.CreateViewModel<LunarEclipseVM>();
            vm.Closing += (e) => { vm.Dispose(); };
            ViewManager.ShowWindow(vm);

        }
    }
}
