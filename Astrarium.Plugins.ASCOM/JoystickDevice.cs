using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.ASCOM
{
    public delegate void ButtonStateChanged(string button, bool state);

    public class JoystickDevice
    {
        public Guid Id { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public bool IsConnected { get; set; }
        public List<JoystickButton> Buttons { get; } = new List<JoystickButton>();

        public override string ToString()
        {
            return Name;
        }
    }
}
