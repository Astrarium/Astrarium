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
            this.joystickManager.ButtonStateChanged += HandleButtonStateChanged;
        }

        public JoystickDevice SelectedDevice
        {
            get => joystickManager.SelectedDevice;
            set
            {
                joystickManager.SelectedDevice = value;
                NotifyPropertyChanged(nameof(SelectedDevice), nameof(ButtonsMappings));
            }
        }

        public ICollection<JoystickDevice> JoystickDevices => joystickManager.Devices;
        public ICollection<JoystickButton> ButtonsMappings => SelectedDevice?.Buttons;

        private void HandleButtonStateChanged(string button, bool isPressed)
        {
            //Task.Run(() =>
            //{
            //    try
            //    {
            //        var mapping = ButtonsMappings.FirstOrDefault(b => b.Button == button);
            //        if (mapping != null)
            //        {
            //            mapping.IsPressed = isPressed;
            //            //if (SelectedTab == Tabs.Telescope)
            //            //{
            //            //    telescope?.ProcessCommand(new ButtonCommand() { Action = mapping.Action, IsPressed = mapping.IsPressed });
            //            //}
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Log.Error($"ERROR: {ex.Message}");
            //    }
            //});
        }
    }
}
