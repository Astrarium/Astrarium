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
        public ICommand EditButtonsMappingCommand { get; private set; }

        private readonly IJoystickManager joystickManager;

        public AscomSettingsViewModel(IJoystickManager joystickManager, ISettings settings) : base(settings)
        {
            this.joystickManager = joystickManager;
            this.joystickManager.DevicesListChanged += () => NotifyPropertyChanged(nameof(JoystickDevices));

            EditButtonsMappingCommand = new Command(EditButtonsMapping);
        }

        public JoystickDevice SelectedDevice
        {
            get => joystickManager.SelectedDevice;
            set
            {
                var oldDevice = joystickManager.SelectedDevice;

                if (oldDevice != null)
                {
                    oldDevice.Buttons.ForEach(x => x.ActionChanged -= ButtonActionChanged);
                }

                joystickManager.SelectedDevice = value;



                Settings.Set("TelescopeControlJoystickDevice", value.Id);


                joystickManager.SelectedDevice.Buttons.ForEach(x => x.ActionChanged += ButtonActionChanged);

                NotifyPropertyChanged(nameof(SelectedDevice), nameof(ButtonsMappings));


            }
        }

        public ICollection<JoystickDevice> JoystickDevices => joystickManager.Devices;
        public ICollection<JoystickButton> ButtonsMappings => SelectedDevice?.Buttons;

        private void ButtonActionChanged(string button, ButtonAction action)
        {
            Settings.Set("TelescopeControlJoystickButtons", joystickManager.SelectedDevice?.Buttons ?? new List<JoystickButton>());
        }

        private void EditButtonsMapping()
        {
            if (ViewManager.ShowDialog<JoystickButtonsMappingVM>() == true)
            {

            }
        }
    }
}
