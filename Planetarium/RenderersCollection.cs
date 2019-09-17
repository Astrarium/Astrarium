using Planetarium.Renderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium
{
    internal class RenderersCollection : List<BaseRenderer>
    {
        public RenderersCollection() : base() { }
        public RenderersCollection(IEnumerable<BaseRenderer> renderers) : base(renderers) { } 
    }
}
