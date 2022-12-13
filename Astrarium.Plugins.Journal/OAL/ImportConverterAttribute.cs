using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.OAL
{
    //[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    //public class ImportConverterAttribute : Attribute
    //{
    //    public string TargetProperty { get; private set; }
    //    public Type ConverterType { get; private set; }

    //    public ImportConverterAttribute(string targetProperty, Type converterType)
    //    {
    //        TargetProperty = targetProperty;
    //        ConverterType = converterType;
    //    }
    //}

    //[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    //public class ExportConverterAttribute : Attribute
    //{
    //    public string TargetProperty { get; private set; }
    //    public Type ConverterType { get; private set; }

    //    public ExportConverterAttribute(string targetProperty, Type converterType)
    //    {
    //        TargetProperty = targetProperty;
    //        ConverterType = converterType;
    //    }
    //}

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OALConverter : Attribute
    {
        public string Property { get; set; }
        public Type ImportConverter { get; set; } = typeof(SimpleConverter);
        public Type ExportConverter { get; set; } = typeof(SimpleConverter);
    }
}
