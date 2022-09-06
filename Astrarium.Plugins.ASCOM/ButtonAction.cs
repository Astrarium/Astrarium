using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.ASCOM
{
    public enum ButtonAction
    {
        [Description("JoystickButtonAction.None")]
        None = 0,

        [Description("JoystickButtonAction.RotatePrimaryReverse")]
        RotatePrimaryReverse = 1,

        [Description("JoystickButtonAction.RotatePrimary")]
        RotatePrimary = 2,

        [Description("JoystickButtonAction.RotateSecondaryReverse")]
        RotateSecondaryReverse = 3,

        [Description("JoystickButtonAction.RotateSecondary")]
        RotateSecondary = 4,

        [Description("JoystickButtonAction.SetMinRotationSpeed")]
        SetMinRotationSpeed = 5,

        [Description("JoystickButtonAction.SetMaxRotationSpeed")]
        SetMaxRotationSpeed = 6,

        [Description("JoystickButtonAction.DecreaseRotationSpeed")]
        DecreaseRotationSpeed = 7,

        [Description("JoystickButtonAction.IncreaseRotationSpeed")]
        IncreaseRotationSpeed = 8,

        [Description("JoystickButtonAction.SwitchTracking")]
        SwitchTracking = 9,

        [Description("JoystickButtonAction.AbortSlewing")]
        AbortSlewing = 10
    }
}
