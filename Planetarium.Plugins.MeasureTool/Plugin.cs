using ADK;
using Planetarium.Config;
using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Plugins.MeasureTool
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

            AddContextMenuItem(new ContextMenuItem("Measure Tool", SwitchMeasureTool, () => true, () => true));
        }

        private void SwitchMeasureTool()
        {
            // switch the ruler
            renderer.IsMeasureToolOn = !renderer.IsMeasureToolOn;

            // if measure starts from a celelstial object, 
            // set beginning of the ruler at center of the object,
            // otherwise start from current mouse position
            renderer.MeasureOrigin = map.SelectedObject != null ?
                new CrdsHorizontal(map.SelectedObject.Horizontal) :
                new CrdsHorizontal(map.MousePosition);

            map.Invalidate();
        }
    }
}
