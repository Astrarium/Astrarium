using System;

namespace Astrarium.Plugins.Journal.Types
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DBStoredAttribute : Attribute
    {
        public Type Entity { get; set; }
        public string Key { get; set; }
        public string Field { get; set; }
    }
}
