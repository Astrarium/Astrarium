using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Meteors
{
    public class MeteorShowersVM : ViewModelBase
    {
        public ICollection<Meteor> Meteors { get; set; }

        public MeteorShowersVM(MeteorsCalculator calc)
        {
            Meteors = calc.Meteors;
        }
    }
}
