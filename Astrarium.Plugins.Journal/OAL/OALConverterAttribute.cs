using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.OAL
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class OALConverterAttribute : Attribute
    {
        public string Property { get; set; }
        public Type ImportConverter { get; set; } = typeof(SimpleConverter);
        public Type ExportConverter { get; set; } = typeof(SimpleConverter);
    }
}
