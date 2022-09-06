using System;
using System.Collections.Generic;

namespace Astrarium.Plugins.ASCOM
{
    public interface IJoystickManager
    {
        bool IsEnabled { get; set; }
        ICollection<JoystickDevice> Devices { get; }
        JoystickDevice SelectedDevice { get; set; }
        event Action<string, bool> ButtonStateChanged;
        event Action DevicesListChanged;
        event Action SelectedDeviceChanged;
    }
}