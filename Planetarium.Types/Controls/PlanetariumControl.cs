using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Planetarium.Types.Controls
{
    public abstract class PlanetariumControl : Control
    {
        public IViewManager ViewManager { get; set; }
    }
}
