using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Types
{
    public class UIElementsConfig<TGroup, TUIElement> 
    {
        private List<Tuple<TGroup, TUIElement>> items = new List<Tuple<TGroup, TUIElement>>();

        private Dictionary<TGroup, int> groupOrders = new Dictionary<TGroup, int>();

        public void Add(TGroup group, TUIElement element)
        {
            items.Add(new Tuple<TGroup, TUIElement>(group, element));
        }

        public void AddRange(UIElementsConfig<TGroup, TUIElement> other)
        {
            items.AddRange(other.items);
        }

        public void SetGroupOrder(TGroup group, int order)
        {
            groupOrders[group] = order;
        }

        public int GetGroupOrder(TGroup group)
        {
            return groupOrders.ContainsKey(group) ? groupOrders[group] : groupOrders.Values.Max() + 1;
        }

        public IEnumerable<TGroup> Groups => items.Select(i => i.Item1).Distinct();
        public IEnumerable<TUIElement> this[TGroup group] => items.Where(i => Equals(group, i.Item1)).Select(i => i.Item2);
    }
}
