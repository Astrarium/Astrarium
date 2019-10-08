using Planetarium.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Planetarium.Types
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
        public Func<SkyContext, T, object> this[string key]
        {
            get
            {
                return Items.FirstOrDefault(i => i.Category == key).Formula as Func<SkyContext, T, object>;
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
                return Items.FirstOrDefault(i => i.Category == key).Formula as Func<SkyContext, T, object>;
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
                return Items.FirstOrDefault(i => i.Category == key).Formula as Func<SkyContext, T, object>;
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
    }

    //public class EphemerisConfigItem<TCelestialObject> : EphemerisConfigItem where TCelestialObject : CelestialObject
    //{
    //    public EphemerisConfigItem(string key, Func<SkyContext, TCelestialObject, object> formula)
    //        : base(key, formula)
    //    {

    //    }

    //    public EphemerisConfigItem(string key, Func<SkyContext, TCelestialObject, object> formula, IEphemFormatter formatter)
    //        : base(key, formula)
    //    {
    //        Formatter = formatter;
    //    }

    //    public EphemerisConfigItem(string key, Func<SkyContext, TCelestialObject, object> formula, Func<TCelestialObject, bool> availableIf)
    //        : base(key, formula)
    //    {
    //        IsAvailable = availableIf;
    //    }        
    //}
}