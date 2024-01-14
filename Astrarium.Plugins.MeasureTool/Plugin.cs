using Astrarium.Algorithms;
using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Astrarium.Plugins.MeasureTool
{
    /// <summary>
    /// Adds ruler tool which allows to measure anglular separation between two points on celestial map.
    /// </summary>
    public class Plugin : AbstractPlugin
    {
        /// <summary>
        /// Map instance
        /// </summary>
        private ISkyMap map;

        /// <summary>
        /// Renderer instance
        /// </summary>
        private MeasureToolRenderer renderer;

        public Plugin(ISkyMap map, MeasureToolRenderer renderer)
        {
            this.map = map;
            this.renderer = renderer;

            var menuItem = new MenuItem("$Astrarium.Plugins.MeasureTool.ContextMenu")
            {
                Command = new Command(SwitchMeasureTool),
                HotKey = new KeyGesture(Key.M, ModifierKeys.Control, "Ctrl+M")
            };
            menuItem.AddBinding(new SimpleBinding(renderer, nameof(renderer.IsMeasureToolOn), "IsChecked"));

            MenuItems.Add(MenuItemPosition.ContextMenu, menuItem);
        }

        private void SwitchMeasureTool()
        {
            // switch the ruler
            renderer.IsMeasureToolOn = !renderer.IsMeasureToolOn;

            // if measure starts from a celelstial object, 
            // set beginning of the ruler at center of the object,
            // otherwise start from current mouse position
            if (renderer.IsMeasureToolOn)
            {
                if (map.SelectedObject != null)
                {
                    renderer.MeasureOrigin = new CrdsEquatorial(map.SelectedObject.Equatorial);
                }
                else
                {
                    renderer.MeasureOrigin = new CrdsEquatorial(map.Projection.WithoutRefraction(map.MouseEquatorialCoordinates));
                }
            }

            map.Invalidate();
        }
    }
}
