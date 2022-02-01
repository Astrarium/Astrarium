using Astrarium.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public override string ToString()
        {
            return $"{Key} = {Formatter?.Format(Value) ?? Value}";
        }
    }

    public class Ephemerides : List<Ephemeris>
    {
        public CelestialObject CelestialObject { get; private set; }

        public Ephemeris Get(string key)
        {
            return this.FirstOrDefault(e => e.Key == key);
        }

        public T GetValue<T>(string key)
        {
            return Get(key).GetValue<T>();
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
