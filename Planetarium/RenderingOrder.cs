using Planetarium.Renderers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium
{    
    internal class RenderingOrder : ObservableCollection<string>
    {
        public RenderingOrder() : base() { }
        public RenderingOrder(IEnumerable<string> items) : base(items) { }
    }
}
