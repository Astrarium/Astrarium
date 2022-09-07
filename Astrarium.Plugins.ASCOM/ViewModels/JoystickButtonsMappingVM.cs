using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Astrarium.Plugins.ASCOM.ViewModels
{
    public class JoystickButtonsMappingVM : ViewModelBase
    {
        private readonly ISettings settings;

        public ICommand CloseCommand { get; private set; }
        public List<JoystickButton> Buttons { get; private set; }

        public JoystickButtonsMappingVM(ISettings settings, IJoystickManager joystickManager)
        {
            CloseCommand = new Command(Close);
            this.settings = settings;
            if (joystickManager.SelectedDevice != null)
            {
                Buttons = joystickManager.SelectedDevice.Buttons;
            }
        }

        public override void Close()
        {
            settings.Set("TelescopeControlJoystickButtons", Buttons);
            base.Close();
        }
    }
}
