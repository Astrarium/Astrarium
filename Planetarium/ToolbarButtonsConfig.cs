using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium
{
    public class ToolbarButtonsConfig : List<ToolbarButton>
    {
        public ToolbarButtonsConfig(ISettings settings)
        {
            // default toolbar buttons going here

            Add(new ToolbarButton("Constellation Lines", "IconConstLines", settings, "ConstLines", "Constellations"));
            Add(new ToolbarButton("Constellation Borders", "IconConstBorders", settings, "ConstBorders", "Constellations"));

            Add(new ToolbarButton("Stars", "IconStars", settings, "Stars", "Objects"));
            
        }
    }
}
