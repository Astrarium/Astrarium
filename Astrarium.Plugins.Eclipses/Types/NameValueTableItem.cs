using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Eclipses.Types
{
    public class NameValueTableItem
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public NameValueTableItem(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
