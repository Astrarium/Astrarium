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
        //public ICommand OkCommand

        private readonly ISettings settings;
        private readonly IJoystickManager joystickManager;

        public JoystickButtonsMappingVM(ISettings settings, IJoystickManager joystickManager)
        {
            this.settings = settings;
            this.joystickManager = joystickManager;

            if (joystickManager.SelectedDevice != null)
            {
                Buttons = joystickManager.SelectedDevice.Buttons;
            }
        }

        public List<JoystickButton> Buttons { get; private set; }

        public override void Close()
        {
            

            base.Close();
        }
    }
}
