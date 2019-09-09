using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Planetarium.Types
{
    public class AstroEventsConfig : IEnumerable<AstroEventsConfigItem>
    {
        protected List<AstroEventsConfigItem> Items { get; } = new List<AstroEventsConfigItem>();

        public IEnumerator<AstroEventsConfigItem> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public ICollection<AstroEventsConfigItem> Filter(ICollection<string> keys)
        {
            return Items.Where(i => keys.Contains(i.Key)).ToArray();
        }

        public AstroEventsConfig Add(string key, Func<AstroEventsContext, ICollection<AstroEvent>> formula)
        {
            Items.Add(new AstroEventsConfigItem(key, formula));
            return this;
        }
    }

    public class AstroEventsConfigItem
    {
        public string Key { get; protected set; }
        public Func<AstroEventsContext, ICollection<AstroEvent>> Formula { get; protected set; }
        
        public AstroEventsConfigItem(string key, Func<AstroEventsContext, ICollection<AstroEvent>> func)
        {
            Key = key;
            Formula = func;
        }
    }
}