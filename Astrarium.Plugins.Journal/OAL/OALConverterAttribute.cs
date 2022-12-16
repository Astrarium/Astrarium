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

        private Type importConverter = typeof(SimpleConverter);

        public Type ImportConverter
        {
            get => importConverter;
            set
            {
                if (importConverter != null && !(importConverter is IOALConverter))
                {
                    throw new ArgumentException($"{nameof(ImportConverter)} should implement {nameof(IOALConverter)} interface");
                }
                importConverter = value;
            }
        }

        public Type ExportConverter { get; set; } = typeof(SimpleConverter);

        public OALConverterAttribute()
        {
            // sanity checks

        }
    }
}
