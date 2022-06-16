using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.ViewModels
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DBStoredAttribute : Attribute
    {
        public Type Entity { get; set; }
        public string Key { get; set; }
        public string Field { get; set; }
    }
}
