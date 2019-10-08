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

        public Func<AstroEventsContext, ICollection<AstroEvent>> this[string key]
        {
            get
            {
                return Items.FirstOrDefault(i => i.Key == key)?.Formula;
            }
            set
            {
                Items.Add(new AstroEventsConfigItem(key, value));
            }
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