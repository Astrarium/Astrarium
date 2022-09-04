using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.ASCOM
{
    public enum ButtonAction
    {
        None = 0,
        RotatePrimaryReverse = 1,
        RotatePrimary = 2,
        RotateSecondaryReverse = 3,
        RotateSecondary = 4,
        SetMinRotationSpeed = 5,
        SetMaxRotationSpeed = 6,
        DecreaseRotationSpeed = 7,
        IncreaseRotationSpeed = 8,
        SwitchTracking = 9,
        AbortSlewing = 10
    }
}
