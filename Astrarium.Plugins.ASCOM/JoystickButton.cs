using Astrarium.Types;
using Newtonsoft.Json;

namespace Astrarium.Plugins.ASCOM
{
    public class JoystickButton : PropertyChangedBase
    {
        [JsonProperty("Button")]
        public string Button { get; set; }

        [JsonProperty("Action")]
        public ButtonAction Action
        {
            get => GetValue<ButtonAction>(nameof(Action));
            set => SetValue(nameof(Action), value);
        }

        [JsonIgnore]
        public bool IsPressed
        {
            get => GetValue<bool>(nameof(IsPressed));
            set => SetValue(nameof(IsPressed), value);
        }
    }
}
