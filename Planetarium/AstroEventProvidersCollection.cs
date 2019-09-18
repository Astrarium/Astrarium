using Planetarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium
{
    public class AstroEventProvidersCollection : List<BaseAstroEventsProvider>
    {
        public AstroEventProvidersCollection() : base() { }
        public AstroEventProvidersCollection(IEnumerable<BaseAstroEventsProvider> providers) : base(providers) { }
    }
}
