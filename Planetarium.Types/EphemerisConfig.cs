using Planetarium.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Planetarium.Types
{
    public abstract class EphemerisConfig
    {
        protected List<EphemerisConfigItem> Items { get; } = new List<EphemerisConfigItem>();

        public ICollection<EphemerisConfigItem> Filter(IEnumerable<string> categories)
        {
            return Items.Where(i => categories.Contains(i.Category)).ToArray();
        }

        public bool IsEmpty
        {
            get { return !Items.Any(); }
        }

        public ICollection<string> GetCategories(CelestialObject body)
        {
            return Items
                .Where(c => (bool?)c.IsAvailable?.DynamicInvoke(body) ?? true)
                .Select(c => c.Category).Distinct().ToArray();
        }
    }

    public class EphemerisConfig<T> : EphemerisConfig where T : CelestialObject
    {
        public Func<SkyContext, T, object> this[string key]
        {
            get
            {
                return Items.FirstOrDefault(i => i.Category == key)?.Formula as Func<SkyContext, T, object>;
            }
            set
            {
                Items.Add(new EphemerisConfigItem(key, value));
            }
        }

        public Func<SkyContext, T, object> this[string key, IEphemFormatter formatter]
        {
            get
            {
                return Items.FirstOrDefault(i => i.Category == key)?.Formula as Func<SkyContext, T, object>;
            }
            set
            {
                Items.Add(new EphemerisConfigItem(key, value, formatter));
            }
        }

        public Func<SkyContext, T, object> this[string key, Func<T, bool> availableIf]
        {
            get
            {
                return Items.FirstOrDefault(i => i.Category == key)?.Formula as Func<SkyContext, T, object>;
            }
            set
            {
                Items.Add(new EphemerisConfigItem(key, value, availableIf));
            }
        }
    }

    public class EphemerisConfigItem
    {
        public string Category { get; private set; }
        public Delegate Formula { get; private set; }
        public IEphemFormatter Formatter { get; private set; }
        public Delegate IsAvailable { get; private set; }

        internal EphemerisConfigItem(string category, Delegate func)
        {
            Category = category;
            Formula = func;
        }

        internal EphemerisConfigItem(string category, Delegate func, IEphemFormatter formatter)
        {
            Category = category;
            Formula = func;
            Formatter = formatter;
        }

        internal EphemerisConfigItem(string category, Delegate func, Delegate availableIf)
        {
            Category = category;
            Formula = func;
            IsAvailable = availableIf;
        }

        internal EphemerisConfigItem(string category, Delegate func, IEphemFormatter formatter, Delegate availableIf)
        {
            Category = category;
            Formula = func;
            Formatter = formatter;
            IsAvailable = availableIf;
        }
    }
}