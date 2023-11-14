using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Meteors
{
    public enum MeteorLabelType
    {
        [Description("MeteorLabelType.Name")]
        Name = 0,

        [Description("MeteorLabelType.Code")]
        Code = 1,
    }
}
