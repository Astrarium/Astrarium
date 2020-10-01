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

            MenuItem eclipsesMenu = new MenuItem("Solar eclipses", new Command(ShowSolarEclipsesView));            
            MenuItems.Add(MenuItemPosition.MainMenuTools, eclipsesMenu);

            #endregion UI integration

            #region Extending formatters

            Formatters.Default["Appearance.CM"] = new Formatters.UnsignedDoubleFormatter(2, "\u00B0");
            Formatters.Default["Appearance.P"] = new Formatters.UnsignedDoubleFormatter(2, "\u00B0");
            Formatters.Default["Appearance.D"] = new Formatters.UnsignedDoubleFormatter(2, "\u00B0");

            #endregion Extending formatters
        }

        private void ShowSolarEclipsesView()
        {
            var vm = ViewManager.CreateViewModel<SolarEclipseVM>();
            ViewManager.ShowDialog(vm);
        }
    }
}
