using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.ObservationsLog.OAL
{
    public static class Mappings
    {
        public static Types.Observer FromOAL(this observerType observer)
        {
            return new Types.Observer()
            {
                Name = observer.name,
                Surname = observer.DSL
            };
        }
    }
}
