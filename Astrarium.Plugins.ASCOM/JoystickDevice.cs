using Astrarium.Types;
using System;
using System.Collections.Generic;

namespace Astrarium.Plugins.ASCOM
{
    public class JoystickDevice : PropertyChangedBase
    {
        public Guid Id { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }

        public bool IsConnected
        {
            get => GetValue<bool>(nameof(IsConnected));
            set => SetValue(nameof(IsConnected), value);
        }

        public List<JoystickButton> Buttons { get; } = new List<JoystickButton>();

        public override string ToString()
        {
            return Name;
        }
    }
}
