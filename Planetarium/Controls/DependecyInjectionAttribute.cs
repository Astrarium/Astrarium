using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planetarium.Controls
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DependecyInjectionAttribute : Attribute { }
}
