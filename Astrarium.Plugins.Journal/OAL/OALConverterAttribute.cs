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
        private Type exportConverter = typeof(SimpleConverter);

        public Type ImportConverter
        {
            get => importConverter;
            set
            {
                if (value != null && !typeof(IOALConverter).IsAssignableFrom(value))
                {
                    throw new ArgumentException($"{ImportConverter.Name} should implement {nameof(IOALConverter)} interface");
                }
                importConverter = value;
            }
        }

        public Type ExportConverter
        {
            get => exportConverter;
            set
            {
                if (value != null && !typeof(IOALConverter).IsAssignableFrom(value))
                {
                    throw new ArgumentException($"{ExportConverter.Name} should implement {nameof(IOALConverter)} interface");
                }
                exportConverter = value;
            }
        }

        public OALConverterAttribute()
        {
            // sanity checks

        }
    }
}
