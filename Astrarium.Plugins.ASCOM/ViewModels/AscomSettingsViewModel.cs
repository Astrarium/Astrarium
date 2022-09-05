using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.ASCOM.ViewModels
{
    public class AscomSettingsViewModel : SettingsViewModel
    {
        private readonly IJoystickManager joystickManager;

        public AscomSettingsViewModel(IJoystickManager joystickManager, ISettings settings) : base(settings)
        {
            this.joystickManager = joystickManager;
            this.joystickManager.DevicesListChanged += () => NotifyPropertyChanged(nameof(JoystickDevices));
        }

        public JoystickDevice SelectedDevice
        {
            get => joystickManager.SelectedDevice;
            set
            {
                joystickManager.SelectedDevice = value;
                Settings.Set("TelescopeControlJoystickDevice", value.Id);
                NotifyPropertyChanged(nameof(SelectedDevice), nameof(ButtonsMappings));
            }
        }

        public ICollection<JoystickDevice> JoystickDevices => joystickManager.Devices;
        public ICollection<JoystickButton> ButtonsMappings => SelectedDevice?.Buttons;
    }
}
