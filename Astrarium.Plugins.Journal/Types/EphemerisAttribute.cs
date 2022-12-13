using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astrarium.Plugins.Journal.Types
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    internal class EphemerisAttribute : Attribute
    {
        public string EphemerisCode { get; private set; }

        public EphemerisAttribute(string ephemerisCode)
        {
            EphemerisCode = ephemerisCode;
        }
    }
}
