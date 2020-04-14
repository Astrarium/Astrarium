using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium
{
    public class UIElementsIntegration
    {
        public UIElementsConfig<string, ToolbarButtonBase> ToolbarButtons { get; } = new UIElementsConfig<string, ToolbarButtonBase>();
        public UIElementsConfig<MenuItemPosition, MenuItem> MenuItems { get; } = new UIElementsConfig<MenuItemPosition, MenuItem>();
    }
}
