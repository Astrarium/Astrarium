using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Astrarium.Plugins.ASCOM.ViewModels
{
    public class AscomSettingsViewModel : SettingsViewModel
    {
        private readonly IJoystickManager joystickManager;

        public ICommand EditButtonsMappingCommand { get; private set; }

        public ICollection<JoystickDevice> JoystickDevices => joystickManager.Devices;

        public AscomSettingsViewModel(IJoystickManager joystickManager, ISettings settings) : base(settings)
        {
            this.joystickManager = joystickManager;
            this.joystickManager.DevicesListChanged += LoadDevices;
            EditButtonsMappingCommand = new Command(EditButtonsMapping);
            LoadDevices();
        }

        private void LoadDevices()
        {
            NotifyPropertyChanged(nameof(JoystickDevices));
        }

        public JoystickDevice SelectedDevice
        {
            get => joystickManager.SelectedDevice;
            set
            {
                joystickManager.SelectedDevice = value;
                Settings.Set("TelescopeControlJoystickDevice", value?.Index ?? 0);
                NotifyPropertyChanged(nameof(SelectedDevice));
            }
        }

        public bool JoystickEnabled
        {
            get => joystickManager.IsEnabled;
            set
            {
                joystickManager.IsEnabled = value;
                Settings.Set("TelescopeControlJoystick", value);
                NotifyPropertyChanged(nameof(JoystickEnabled));
            }
        }

        private void EditButtonsMapping()
        {
            ViewManager.ShowDialog<JoystickButtonsMappingVM>();
        }
    }
}
