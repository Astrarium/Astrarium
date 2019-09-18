using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium
{
    public class CalculatorsCollection : List<BaseCalc>
    {
        public CalculatorsCollection() : base() { }
        public CalculatorsCollection(IEnumerable<BaseCalc> calcs) : base(calcs) { }
    }
}
