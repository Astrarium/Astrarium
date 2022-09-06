using Astrarium.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.ASCOM
{
    public class JoystickButton : PropertyChangedBase
    {
        public event Action<string, ButtonAction> ActionChanged;

        [JsonProperty("Button")]
        public string Button { get; set; }

        [JsonProperty("Action")]
        public ButtonAction Action
        {
            get => GetValue<ButtonAction>(nameof(Action));
            set
            {
                SetValue(nameof(Action), value);
                ActionChanged?.Invoke(Button, value);
            }
        }

        [JsonIgnore]
        public bool IsPressed
        {
            get => GetValue<bool>(nameof(IsPressed));
            set => SetValue(nameof(IsPressed), value);
        }
    }
}
