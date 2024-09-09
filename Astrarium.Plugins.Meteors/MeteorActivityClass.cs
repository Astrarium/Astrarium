using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Meteors
{
    public enum MeteorActivityClass
    {
        [Description("MeteorActivityClass.I")]
        I = 1,

        [Description("MeteorActivityClass.II")]
        II = 2,

        [Description("MeteorActivityClass.III")]
        III = 3,

        [Description("MeteorActivityClass.IV")]
        IV = 4
    }
}
