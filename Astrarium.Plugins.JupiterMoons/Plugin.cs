using Astrarium.Types;
using System;

namespace Astrarium.Plugins.JupiterMoons
{
    public class Plugin : AbstractPlugin
    {
        public Plugin()
        {
            MenuItems.Add(MenuItemPosition.MainMenuTools, 
                new MenuItem("$Astrarium.Plugins.JupiterMoons.ToolsMenu", 
                new Command(() => ViewManager.ShowWindow<JupiterMoonsVM>(isSingleInstance: true))));
        }
    }
}
