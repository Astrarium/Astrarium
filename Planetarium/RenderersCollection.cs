using Planetarium.Renderers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium
{
    internal class RenderersCollection : List<BaseRenderer>
    {
        private RenderingOrder Order;

        private Comparison<BaseRenderer> comparison;

        public RenderersCollection() : this(new BaseRenderer[] { })
        {

        }

        public RenderersCollection(IEnumerable<BaseRenderer> renderers) : base(renderers)
        {
            comparison = new Comparison<BaseRenderer>((r1, r2) => {
                int i1 = Order.IndexOf(Order.FirstOrDefault(ro => ro.RendererType == r1.GetType().FullName));
                int i2 = Order.IndexOf(Order.FirstOrDefault(ro => ro.RendererType == r2.GetType().FullName));
                return i1 - i2;
            });
        }

        public void Sort(RenderingOrder order)
        {
            Order = order;
            Sort(comparison);
        }
    }
}
