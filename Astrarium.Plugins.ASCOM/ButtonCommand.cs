using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.ASCOM
{
    public class ButtonCommand
    {
        public ButtonAction Action { get; set; }
        public bool IsPressed { get; set; }
    }
}
