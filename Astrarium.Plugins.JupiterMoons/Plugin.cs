using Astrarium.Types;
using System;

namespace Astrarium.Plugins.JupiterMoons
{
    public class Plugin : AbstractPlugin
    {
        public Plugin()
        {
            var menuItem = new MenuItem("Jupiter Moons", new Command(ShowJupiterMoonsView));
            MenuItems.Add(MenuItemPosition.MainMenuTools, menuItem);
        }

        private void ShowJupiterMoonsView()
        {
            ViewManager.ShowWindow<JupiterMoonsVM>();
        }
    }
}
