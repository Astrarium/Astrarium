using Planetarium.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Planetarium
{
    public abstract class EphemerisConfig : IEnumerable<EphemerisConfigItem>
    {
        internal List<EphemerisConfigItem> Items { get; } = new List<EphemerisConfigItem>();

        public IEnumerator<EphemerisConfigItem> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public ICollection<EphemerisConfigItem> Filter(IEnumerable<string> categories)
        {
            return Items.Where(i => categories.Contains(i.Category)).ToArray();
        }
    }

    public class EphemerisConfig<T> : EphemerisConfig where T : CelestialObject
    {
        public EphemerisConfigItem<T, R> Add<R>(string key, Func<SkyContext, T, R> formula)
        {
            var item = new EphemerisConfigItem<T, R>(key, formula);
            Items.Add(item);
            return item;
        }
    }

    public abstract class EphemerisConfigItem
    {
        public string Category { get; protected set; }
        public Delegate Formula { get; protected set; }
        public IEphemFormatter Formatter { get; protected set; }
        public Func<CelestialObject, bool> IsAvailable { get; protected set; }

        protected EphemerisConfigItem(string category, Delegate func)
        {
            Category = category;
            Formula = func;
        }
    }

    public class EphemerisConfigItem<TCelestialObject, TResult> : EphemerisConfigItem where TCelestialObject : CelestialObject
    {
        public EphemerisConfigItem(string key, Func<SkyContext, TCelestialObject, TResult> formula)
            : base(key, formula)
        {

        }

        public EphemerisConfigItem<TCelestialObject, TResult> WithFormatter(IEphemFormatter formatter)
        {
            Formatter = formatter;
            return this;
        }

        public EphemerisConfigItem<TCelestialObject, TResult> AvailableIf(Func<CelestialObject, bool> condition)
        {
            IsAvailable = condition;
            return this;
        }
    }
}