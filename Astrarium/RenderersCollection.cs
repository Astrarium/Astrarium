using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium
{
    internal class RenderersCollection : List<BaseRenderer>
    {
        public RenderersCollection() : base() { }
        public RenderersCollection(IEnumerable<BaseRenderer> renderers) : base(renderers) { }

        public void Sort(IEnumerable<string> order)
        {
            Sort(new RenderersOrderComparer(order));
        }

        private class RenderersOrderComparer : IComparer<BaseRenderer>
        {
            private IList<string> Order;

            public RenderersOrderComparer(IEnumerable<string> order)
            {
                Order = new List<string>(order);
            }

            public int Compare(BaseRenderer r1, BaseRenderer r2)
            {
                int i1 = GetOrderIndex(r1);
                int i2 = GetOrderIndex(r2);
                if (i1 == -1 && i2 == -1)
                {
                    return r1.Order - r2.Order;
                }
                else
                {
                    return i1 - i2;
                }
            }

            private int GetOrderIndex(BaseRenderer renderer)
            {
                return Order.IndexOf(Order.FirstOrDefault(ro => ro == renderer.GetType().FullName));
            }
        }
    }
}
