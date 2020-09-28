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
    }

    public class Ephemerides : List<Ephemeris>
    {
        public Ephemeris Get(string key)
        {
            return this.FirstOrDefault(e => e.Key == key);
        }

        public T GetValue<T>(string key)
        {
            return (T)Get(key)?.Value;
        }
    }
}
