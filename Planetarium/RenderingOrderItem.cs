using Planetarium.Renderers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium
{
    internal class RenderingOrderItem
    {
        public string Name { get; set; }
        public string RendererType { get; set; }

        public RenderingOrderItem() { }
        public RenderingOrderItem(BaseRenderer renderer)
        {
            Name = renderer.Name;
            RendererType = renderer.GetType().FullName;
            // TODO: include version, description, author name, and etc.
        }
    }

    internal class RenderingOrder : ObservableCollection<RenderingOrderItem>
    {
        public RenderingOrder() : base() { }
        public RenderingOrder(IEnumerable<RenderingOrderItem> items) : base(items) { }
    }
}
