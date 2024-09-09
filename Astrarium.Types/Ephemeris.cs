using System.Collections.Generic;
using System.Linq;

namespace Astrarium.Types
{
    public class Ephemeris
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public IEphemFormatter Formatter { get; set; }
        public T GetValue<T>()
        {
            return (T)Value;
        }

        public Ephemeris(string key, object value, IEphemFormatter formatter = null)
        {
            Key = key;
            Value = value;
            Formatter = formatter ?? Formatters.GetDefault(key);
        }

        public override string ToString()
        {
            return $"{Key} = {Formatter?.Format(Value) ?? Value}";
        }
    }

    public class Ephemerides : List<Ephemeris>
    {
        public CelestialObject CelestialObject { get; private set; }

        public object this[string key]
        {
            get => GetValue<object>(key);
        }

        public Ephemeris Get(string key)
        {
            return this.FirstOrDefault(e => e.Key == key);
        }

        public T GetValue<T>(string key)
        {
            return Get(key).GetValue<T>();
        }

        public T GetValueOrDefault<T>(string key)
        {
            var ephem = Get(key);
            return (ephem != null) ? ephem.GetValue<T>() : default(T);
        }

        public Ephemerides(CelestialObject body)
        {
            CelestialObject = body;
        }

        public override string ToString()
        {
            return $"{CelestialObject.Names.First()} {base.ToString()}";
        }
    }
}
